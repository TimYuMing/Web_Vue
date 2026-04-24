using Web_Vue.Server.Interfaces;
using Web_Vue.Server.Models;
using Web_Vue.Server.Models.Entities;
using Web_Vue.Server.Repositories;
using Web_Vue.Server.Services;

namespace Web_Vue.Server._Authorization;

/// <summary> 權限驗證 Handler </summary>
public class PermissionAuthHandler(
    DbEntityContext _context,
    IBaseRepository<Role> _roleRe,
    IBaseRepository<Permission> _permissionRe,
    CurrentUserService _currentUserService
) : AuthorizationHandler<PermissionAuthRequirement>
{
    /// <inheritdoc/>
    protected override async Task<Task> HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionAuthRequirement requirement)
    {
        var userInfo = _currentUserService.UserInfo;

        if (userInfo == null)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        // 系統管理者直接通過
        if (!userInfo.IsAdmin)
        {
            var permissionInRoleRe = new PermissionInRoleRepository(_context);

            var userPermissions = await (
                from role in _roleRe.FindAll()
                join permissionInRole in permissionInRoleRe.FindAll() on role.ID equals permissionInRole.RoleID
                join permission in _permissionRe.FindAll() on permissionInRole.PermissionID equals permission.ID
                where role.IsEnable && userInfo.RoleList.Contains(role.ID) && permission.IsEnable
                select permission.Code
            ).ToListAsync();

            if (!userPermissions.Any(y => requirement.PermissionCodeList.Contains(y)))
            {
                context.Fail();
                return Task.CompletedTask;
            }
        }

        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
