namespace Web_Vue.Server.ViewModels.Auth;

/// <summary> 登入圖形驗證碼 ViewModel </summary>
public class CaptchaViewModel
{
    /// <summary> Challenge ID（呼叫登入 API 時需一併帶入） </summary>
    public string ChallengeId { get; set; } = string.Empty;

    /// <summary> SVG 驗證碼圖片 data URI (data:image/svg+xml;base64,...) </summary>
    public string ImageBase64 { get; set; } = string.Empty;

    /// <summary> 過期時間 (UTC) </summary>
    public DateTime ExpiresAt { get; set; }
}
