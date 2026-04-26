namespace Web_Vue.Server.ViewModels.Base;

/// <summary>
/// 全域靜態設定常數
/// — 存放與環境無關、不從 appsettings 讀取的系統行為常數
/// </summary>
public static class Config
{
    // ===================== 登入安全 =====================

    /// <summary> 連續登入失敗後禁止登入的分鐘數 </summary>
    public const int LoginFailBanMinutes = 15;

    /// <summary> 累積失敗幾次後觸發鎖定 </summary>
    public const int LoginFailToBanCount = 5;

    /// <summary> 登入失敗狀態在快取中保留的小時數 </summary>
    public const int LoginAttemptStateTtlHours = 24;

    /// <summary> 圖形驗證碼有效分鐘數 </summary>
    public const int CaptchaTtlMinutes = 5;

    /// <summary> 圖形驗證碼位數 </summary>
    public const int CaptchaCodeLength = 4;

    // ===================== 快取 Key 前綴 =====================

    /// <summary> 登入 Challenge（圖形驗證碼）快取 Key 前綴 </summary>
    public const string ChallengeCacheKeyPrefix = "AuthLoginChallenge";

    /// <summary> 登入失敗次數快取 Key 前綴 </summary>
    public const string AttemptCacheKeyPrefix = "ApiLoginAttempt";

    // ===================== Cookie 名稱 =====================

    /// <summary> JWT 驗證 Cookie 名稱（HttpOnly，用於伺服器端驗證） </summary>
    public const string JwtCookieName = "jwt-token";

    /// <summary> CSRF Token Cookie 名稱（非 HttpOnly，由 JS 讀取後帶入 Header） </summary>
    public const string CsrfCookieName = "csrf-token";

    /// <summary> CSRF Token Header 名稱（JS 每次請求時帶入此 Header） </summary>
    public const string CsrfHeaderName = "X-CSRF-Token";
}
