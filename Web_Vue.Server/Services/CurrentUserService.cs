using Web_Vue.Server.Models.Auth;
using Web_Vue.Server.Tools;

namespace Web_Vue.Server.Services;

/// <summary> 從 HttpContext 讀取 JWT Claims，取得當前登入使用者完整資訊 </summary>
public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    /// <inheritdoc/>
    public UserInfoModel? UserInfo { get; } =
        UserInfoTool.GetUserInfo(httpContextAccessor.HttpContext?.User);

    /// <inheritdoc/>
    public bool IsAuthenticated => UserInfo?.IsLogin ?? false;
}
