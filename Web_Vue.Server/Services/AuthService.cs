using Mapster;
using Web_Vue.Server.Interfaces;
using Web_Vue.Server.Models;
using Web_Vue.Server.Models.Entities;
using Web_Vue.Server.ViewModels.Auth;

namespace Web_Vue.Server.Services;

/// <summary>
/// 身份驗證相關 DB 邏輯（使用 Repository pattern）
/// — UserAccount / UserProfile 透過 IBaseRepository 存取
/// — RoleInUserAccount / PermissionInRole 為複合主鍵關聯表，直接透過 DbContext 存取
/// </summary>
public class AuthService(
    IBaseRepository<UserAccount> _userAccountRe,
    IBaseRepository<UserProfile> _userProfileRe,
    IBaseRepository<Role> _roleRe,
    IBaseRepository<Permission> _permissionRe,
    DbEntityContext _dbContext)
{
    /// <summary> 依帳號字串查詢並組建 UserInfoViewModel（含角色） </summary>
    public async Task<UserInfoViewModel?> GetUserInfoByAccountAsync(string account)
    {
        var id = await _userAccountRe.FindAll()
                                     .Where(u => u.Account == account)
                                     .Select(u => (int?)u.ID)
                                     .FirstOrDefaultAsync();

        return id.HasValue ? await GetUserInfoByIdAsync(id.Value) : null;
    }

    /// <summary> 依使用者 ID 查詢並組建 UserInfoViewModel（含角色） </summary>
    public async Task<UserInfoViewModel?> GetUserInfoByIdAsync(int id)
        => await (
            from userAccount in _userAccountRe.FindAll()
            join userProfile in _userProfileRe.FindAll() on userAccount.ID equals userProfile.ID
            where userAccount.ID == id

            let roleData = (from roleInUser in _dbContext.RoleInUserAccount
                            join role in _roleRe.FindAll(false) on roleInUser.RoleID equals role.ID
                            where roleInUser.UserAccountID == userAccount.ID && role.IsEnable
                            select new { roleInUser.RoleID, role.IsAdmin, role.IsMaintaining }).ToList()

            select new UserInfoViewModel
            {
                ID = userAccount.ID,
                Account = userAccount.Account,
                LatestLoginDate = userAccount.LatestLoginDate,
                IsTemplatePassword = userAccount.IsTemplatePassword,
                ValidPasswordExpireTime = userAccount.ValidPasswordExpireTime,
                Name = userProfile.Name,
                Status = userProfile.Status,
                RoleList = roleData.Select(r => r.RoleID).Distinct().ToList(),
                IsAdmin = roleData.Any(r => r.IsAdmin),
                IsMaintaining = roleData.Any(r => r.IsMaintaining),
            }
        ).FirstOrDefaultAsync();

    /// <summary> 組建包含權限清單的 AuthSessionViewModel </summary>
    public async Task<AuthSessionViewModel> BuildSessionAsync(UserInfoViewModel userInfo)
    {
        List<string> permissions;

        if (userInfo.IsAdmin)
        {
            permissions = await (
                from permission in _permissionRe.FindAll()
                where permission.IsEnable
                select permission.Code
            ).Distinct().ToListAsync();
        }
        else if (userInfo.RoleList.Count > 0)
        {
            permissions = await (
                from permissionInRole in _dbContext.PermissionInRole
                join permission in _permissionRe.FindAll() on permissionInRole.PermissionID equals permission.ID
                join role in _roleRe.FindAll() on permissionInRole.RoleID equals role.ID
                where userInfo.RoleList.Contains(permissionInRole.RoleID) && permission.IsEnable && role.IsEnable
                select permission.Code
            ).Distinct().ToListAsync();
        }
        else
        {
            permissions = [];
        }

        return new AuthSessionViewModel
        {
            User = userInfo.Adapt<AuthUserViewModel>(),
            Permissions = permissions,
        };
    }

    /// <summary> 記錄最後登入時間 </summary>
    public async Task RecordLoginAsync(int userAccountId)
    {
        var userAccount = await _userAccountRe.FindAll()
                                              .Where(u => u.ID == userAccountId)
                                              .FirstOrDefaultAsync();
        if (userAccount == null)
        {
            return;
        }

        userAccount.LatestLoginDate = DateTime.UtcNow;
        await _userAccountRe.UpdateAsync(userAccount);
    }
}
