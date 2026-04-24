using Web_Vue.Server.ViewModels.Auth;
using Web_Vue.Server.Tools;

namespace Web_Vue.Server.Services;

/// <summary> 登入 Challenge（圖形驗證碼）服務 </summary>
public class LoginChallengeService(CacheService _cacheService)
{
    private const string CacheKeyPrefix = "AuthLoginChallenge";
    private static readonly TimeSpan ChallengeTtl = TimeSpan.FromMinutes(5);

    /// <summary> 建立新的驗證碼 Challenge </summary>
    public CaptchaViewModel Create()
    {
        var challengeId = Guid.NewGuid().ToString("N");
        var code = CaptchaTool.GenerateCode(4);

        _cacheService.SetCache(BuildKey(challengeId), code, ChallengeTtl);

        return new CaptchaViewModel
        {
            ChallengeId = challengeId,
            ImageBase64 = CaptchaTool.GenerateSvgBase64(code),
            ExpiresAt = DateTime.UtcNow.Add(ChallengeTtl),
        };
    }

    /// <summary> 驗證並消耗 Challenge（只能用一次） </summary>
    public bool ValidateAndConsume(string? challengeId, string? inputCode)
    {
        if (string.IsNullOrWhiteSpace(challengeId) || string.IsNullOrWhiteSpace(inputCode))
        {
            return false;
        }

        var key = BuildKey(challengeId);
        var storedCode = _cacheService.GetCache<string>(key);

        if (string.IsNullOrWhiteSpace(storedCode))
        {
            return false;
        }

        var isValid = string.Equals(storedCode, inputCode.Trim(), StringComparison.OrdinalIgnoreCase);
        if (isValid)
        {
            _cacheService.RemoveCache(key);
        }

        return isValid;
    }

    private static string BuildKey(string challengeId) => $"{CacheKeyPrefix}_{challengeId}";
}
