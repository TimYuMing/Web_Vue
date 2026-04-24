namespace Web_Vue.Server.Tools;

/// <summary>
/// 密碼雜湊輔助工具，使用 BCrypt 演算法
/// </summary>
public static class BCryptTool
{
    /// <summary>
    /// 將明文密碼雜湊化
    /// </summary>
    public static string HashPassword(string password)
        => BCrypt.Net.BCrypt.HashPassword(password);

    /// <summary>
    /// 驗證密碼是否與雜湊值相符
    /// </summary>
    public static bool VerifyPassword(string password, string hashedPassword)
        => BCrypt.Net.BCrypt.Verify(password, hashedPassword);
}
