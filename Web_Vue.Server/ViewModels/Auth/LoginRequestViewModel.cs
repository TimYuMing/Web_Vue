namespace Web_Vue.Server.ViewModels.Auth;

/// <summary> 登入請求 DTO </summary>
public class LoginRequestViewModel
{
    /// <summary> 使用者帳號 </summary>
    public string Account { get; set; } = string.Empty;

    /// <summary> 密碼 </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary> 驗證碼 Challenge ID（由 /api/auth/captcha 取得） </summary>
    public string ChallengeId { get; set; } = string.Empty;

    /// <summary> 使用者輸入的圖形驗證碼 </summary>
    public string ValidCode { get; set; } = string.Empty;
}
