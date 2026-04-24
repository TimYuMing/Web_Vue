namespace Web_Vue.Server;

/// <summary>
/// JWT 驗證設定（對應 appsettings.json 的 JwtSettings 節點）
/// 使用 IOptions&lt;JwtSettings&gt; 注入，避免各處重複讀取 IConfiguration
/// </summary>
public class JwtSettings
{
    public string SecretKey    { get; set; } = string.Empty;
    public string Issuer       { get; set; } = string.Empty;
    public string Audience     { get; set; } = string.Empty;
    public int    ExpireMinutes { get; set; } = 60;
}
