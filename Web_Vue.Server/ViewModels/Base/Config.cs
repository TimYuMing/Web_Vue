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
}
