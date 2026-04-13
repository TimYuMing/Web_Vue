namespace Web_Vue.Server.Models.Auth;

/// <summary> 登入請求 DTO </summary>
public class LoginRequest
{
    /// <summary> 使用者帳號 </summary>
    public string Account { get; set; } = string.Empty;

    /// <summary> 密碼 </summary>
    public string Password { get; set; } = string.Empty;
}
