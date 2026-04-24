namespace Web_Vue.Server.ViewModels.Base;

/// <summary>
/// 系統模式（對應 appsettings.json SystemInfo:SystemMode）
/// </summary>
public enum SystemMode
{
    /// <summary> 開發環境 </summary>
    Development = 1,
    /// <summary> 測試環境 </summary>
    Testing = 2,
    /// <summary> 正式環境 </summary>
    Production = 3,
}

/// <summary>
/// 對應 appsettings.json 根層設定，統一透過 IOptions&lt;AppSettings&gt; 注入
/// </summary>
public class AppSettings
{
    /// <summary> 系統資訊 </summary>
    public SystemInfoSettings SystemInfo { get; set; } = new();

    /// <summary> JWT 驗證設定 </summary>
    public JwtSettings JwtSettings { get; set; } = new();

    /// <summary> 跨來源請求設定 </summary>
    public CorsSettings Cors { get; set; } = new();
}

/// <summary> 系統資訊（對應 appsettings.json SystemInfo 節點） </summary>
public class SystemInfoSettings
{
    /// <summary> 系統標題 </summary>
    public string SystemTitle { get; set; } = string.Empty;

    /// <summary> 系統模式 </summary>
    public SystemMode SystemMode { get; set; } = SystemMode.Development;

    /// <summary> 是否固定驗證碼（測試環境用） </summary>
    public bool FixCaptcha { get; set; } = false;
}

/// <summary> JWT 驗證設定（對應 appsettings.json JwtSettings 節點） </summary>
public class JwtSettings
{
    public string SecretKey     { get; set; } = string.Empty;
    public string Issuer        { get; set; } = string.Empty;
    public string Audience      { get; set; } = string.Empty;
    public int    ExpireMinutes { get; set; } = 60;
}

/// <summary> CORS 設定（對應 appsettings.json Cors 節點） </summary>
public class CorsSettings
{
    /// <summary> 允許的來源清單 </summary>
    public List<string> AllowedOrigins { get; set; } = [];
}
