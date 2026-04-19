using System.Net;
using Microsoft.AspNetCore.Mvc.Filters;
using Web_Vue.Server.Services;

namespace Web_Vue.Server._Authorization;

/// <summary> API 權限 Filter（回傳 JSON，不做 Redirect） </summary>
public class ApiPermissionFilter : Attribute, IAsyncAuthorizationFilter
{
    private readonly List<string> _permissionCodeList;

    /// <summary> 建構子 </summary>
    /// <param name="permissionCodes"> 所需的權限代碼（至少符合一項即通過） </param>
    public ApiPermissionFilter(params string[] permissionCodes)
    {
        _permissionCodeList = permissionCodes?.ToList() ?? [];
    }

    /// <summary> 非同步驗證方法 </summary>
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var sp = context.HttpContext.RequestServices;
        var authService = sp.GetRequiredService<IAuthorizationService>();
        var currentUserService = sp.GetRequiredService<ICurrentUserService>();
        var resx = sp.GetRequiredService<IStringLocalizer<SharedResource>>();

        var userInfo = currentUserService.UserInfo;

        // 未登入
        if (userInfo is not { IsLogin: true })
        {
            context.Result = new ObjectResult(new ResponseViewModel
            {
                Status = ResultType.Fail,
                Message = resx["SysMsg_VerifyError_未登入"].Value
            })
            {
                StatusCode = (int)HttpStatusCode.Unauthorized
            };
            return;
        }

        // 系統維護中
        if (userInfo.IsMaintaining)
        {
            context.Result = new ObjectResult(new ResponseViewModel
            {
                Status = ResultType.Fail,
                Message = resx["Txt_系統維護中"].Value
            })
            {
                StatusCode = (int)HttpStatusCode.Locked
            };
            return;
        }

        // 權限驗證
        var authResult = await authService.AuthorizeAsync(
            context.HttpContext.User, null, new PermissionAuthRequirement(_permissionCodeList));

        if (!authResult.Succeeded)
        {
            context.Result = new ObjectResult(new ResponseViewModel
            {
                Status = ResultType.Fail,
                Message = resx["RedirectMessage_Base_您無權限進入"].Value
            })
            {
                StatusCode = (int)HttpStatusCode.Forbidden
            };
        }
    }
}
