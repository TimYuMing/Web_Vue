using Web_Vue.Server.Models.Auth;

namespace Web_Vue.Server.Services;

/// <summary> JWT Token 服務介面 </summary>
public interface IJwtService
{
    /// <summary> 依據使用者資訊模型產生 JWT Token </summary>
    string GenerateToken(UserInfoModel userInfo);
}
