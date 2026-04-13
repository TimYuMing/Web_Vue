using System.Security.Cryptography;

namespace Web_Vue.Server.Helpers;

/// <summary>
/// 密碼雜湊輔助工具，使用 PBKDF2-SHA256 演算法
/// </summary>
public static class PasswordHelper
{
    private const int SaltSize   = 16;
    private const int HashSize   = 32;
    private const int Iterations = 100_000;

    /// <summary>
    /// 將明文密碼雜湊化，回傳格式為 <c>base64(salt).base64(hash)</c>
    /// </summary>
    public static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// 以固定時間比較驗證密碼是否與雜湊值相符，防止 timing-attack
    /// </summary>
    public static bool VerifyPassword(string password, string hashedPassword)
    {
        var parts = hashedPassword.Split('.');
        if (parts.Length != 2) return false;

        var salt         = Convert.FromBase64String(parts[0]);
        var expectedHash = Convert.FromBase64String(parts[1]);
        var actualHash   = Rfc2898DeriveBytes.Pbkdf2(
            password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

        return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
    }
}
