using Web_Vue.Server.Models.Auth;

namespace Web_Vue.Server.Services;

/// <summary> 當前登入使用者服務介面 </summary>
public interface ICurrentUserService
{
    /// <summary> 登入者完整資訊（從 JWT Claims 反序列化），未登入時為 null </summary>
    UserInfoModel? UserInfo { get; }

    /// <summary> 是否已通過身份驗證 </summary>
    bool IsAuthenticated { get; }
}
