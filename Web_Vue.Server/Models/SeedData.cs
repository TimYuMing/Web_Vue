using Web_Vue.Server.Models.Entities;

namespace Web_Vue.Server.Models;

/// <summary>
/// 初始化
/// </summary>
public static class SeedData
{
    /// <summary>
    /// 初始化寫入資料
    /// </summary>
    public static async Task InitializeAsync(DbEntityContext context)
    {
        var isInit = await context.UserAccount.AnyAsync();
        if (!isInit)
        {
            await InitUserAccount(context);
        }
    }

    /// <summary>
    /// 初始化使用者帳號
    /// </summary>
    private static async Task InitUserAccount(DbEntityContext context)
    {
        var userAccount = new UserAccount
        {
            Account = "admin",
            Password = "$2a$11$gj.bWhZfJpDje7R9SRCerO4Ndi54nZEiXkHm1mqKCefUg0dXWrYum",
            LatestLoginDate = DateTime.UtcNow,
            ValidPasswordExpireTime = null,
            IsTemplatePassword = false
        };

        await context.UserAccount.AddAsync(userAccount);
        await context.SaveChangesAsync();
        var adminID = userAccount.ID;

        var userProfile = new UserProfile
        {
            ID = adminID,
            Name = "總管理者",
            Status = UserProfileStatus.Enable
        };

        await context.UserProfile.AddAsync(userProfile);
        await context.SaveChangesAsync();

        var roleAdmin = new Role
        {
            Name = "超級管理者",
            Code = "SystemAdmin",
            Description = string.Empty,
            IsAdmin = true,
            IsEnable = true,
            IsMaintaining = false
        };
        var role = new Role
        {
            Name = "系統管理者",
            Code = "Admin",
            Description = string.Empty,
            IsAdmin = true,
            IsEnable = true,
            IsMaintaining = false
        };

        await context.Role.AddRangeAsync(roleAdmin, role);
        await context.SaveChangesAsync();

        var roleInUserAccount = new RoleInUserAccount
        {
            UserAccountID = adminID,
            RoleID = roleAdmin.ID
        };
        await context.RoleInUserAccount.AddAsync(roleInUserAccount);
        await context.SaveChangesAsync();
    }
}
