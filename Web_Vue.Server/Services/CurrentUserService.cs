using Web_Vue.Server.ViewModels.Auth;
using Web_Vue.Server.Tools;

namespace Web_Vue.Server.Services;

/// <summary> 從 HttpContext 讀取 JWT Claims，取得當前登入使用者完整資訊 </summary>
public class CurrentUserService(IHttpContextAccessor _httpContextAccessor)
{
    /// <summary> 登入者完整資訊（從 JWT Claims 反序列化），未登入時為 null </summary>
    public UserInfoViewModel? UserInfo { get; } =
        UserInfoTool.GetUserInfo(_httpContextAccessor.HttpContext?.User);

    /// <summary> 是否已通過身份驗證 </summary>
    public bool IsAuthenticated => UserInfo?.IsLogin ?? false;
}
