using Web_Vue.Server.Tools;
using Web_Vue.Server.ViewModels.Auth;
using Web_Vue.Server.ViewModels.Base;

namespace Web_Vue.Server.Services;

/// <summary> 身份驗證服務（圖形驗證碼 Challenge + 登入失敗鎖定） </summary>
public class AuthService(CacheTool _cacheTool)
{
    // ===================== 登入 Challenge（圖形驗證碼） =====================

    private static readonly TimeSpan ChallengeTtl = TimeSpan.FromMinutes(Config.CaptchaTtlMinutes);

    /// <summary> 建立新的驗證碼 Challenge </summary>
    public CaptchaViewModel CreateChallenge()
    {
        var challengeId = Guid.NewGuid().ToString("N");
        var code = CaptchaTool.GenerateCode(Config.CaptchaCodeLength);

        _cacheTool.SetCache($"{Config.ChallengeCacheKeyPrefix}_{challengeId}", code, ChallengeTtl);

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

        var key = $"{Config.ChallengeCacheKeyPrefix}_{challengeId}";
        var storedCode = _cacheTool.GetCache<string>(key);

        if (string.IsNullOrWhiteSpace(storedCode))
        {
            return false;
        }

        var isValid = string.Equals(storedCode, inputCode.Trim(), StringComparison.OrdinalIgnoreCase);
        if (isValid)
        {
            _cacheTool.RemoveCache(key);
        }

        return isValid;
    }

    // ===================== 登入失敗鎖定 =====================

    private const string AttemptCacheKeyPrefix = Config.AttemptCacheKeyPrefix;
    private const int MaxFailCount = Config.LoginFailToBanCount;
    private static readonly TimeSpan BanDuration = TimeSpan.FromMinutes(Config.LoginFailBanMinutes);
    private static readonly TimeSpan StateTtl    = TimeSpan.FromHours(Config.LoginAttemptStateTtlHours);

    /// <summary> 目前是否鎖定中 </summary>
    public bool IsLocked(string account, string clientIp)
    {
        var state = GetAttemptState(account, clientIp);
        return state.BanUntilUtc.HasValue && state.BanUntilUtc.Value > DateTime.UtcNow;
    }

    /// <summary> 紀錄一次失敗，達上限後鎖定 </summary>
    public void RegisterFailure(string account, string clientIp)
    {
        var key   = BuildAttemptKey(account, clientIp);
        var state = GetAttemptState(account, clientIp);
        var now   = DateTime.UtcNow;

        // 已鎖定中，不重複計數
        if (state.BanUntilUtc.HasValue && state.BanUntilUtc.Value > now)
        {
            return;
        }

        state.FailCount++;
        if (state.FailCount >= MaxFailCount)
        {
            state.BanUntilUtc = now.Add(BanDuration);
        }

        _cacheTool.SetCache(key, state, StateTtl);
    }

    /// <summary> 清除失敗紀錄（登入成功後呼叫） </summary>
    public void ResetAttempt(string account, string clientIp)
        => _cacheTool.RemoveCache(BuildAttemptKey(account, clientIp));

    private LoginAttemptState GetAttemptState(string account, string clientIp)
        => _cacheTool.GetCache<LoginAttemptState>(BuildAttemptKey(account, clientIp)) ?? new LoginAttemptState();

    private static string BuildAttemptKey(string account, string clientIp)
    {
        var safeAccount = string.IsNullOrWhiteSpace(account)
            ? "__anonymous"
            : account.Trim().ToLowerInvariant();
        var safeIp = string.IsNullOrWhiteSpace(clientIp) ? "unknown" : clientIp.Trim();
        return $"{AttemptCacheKeyPrefix}_{safeAccount}_{safeIp}";
    }

    private sealed class LoginAttemptState
    {
        public int FailCount { get; set; }
        public DateTime? BanUntilUtc { get; set; }
    }
}
