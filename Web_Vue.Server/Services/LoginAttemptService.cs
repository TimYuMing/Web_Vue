namespace Web_Vue.Server.Services;

/// <summary> 登入失敗鎖定服務（依帳號 + IP） </summary>
public class LoginAttemptService(CacheService _cacheService)
{
    private const string CacheKeyPrefix = "ApiLoginAttempt";
    private const int MaxFailCount = 5;
    private static readonly TimeSpan BanDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan StateTtl    = TimeSpan.FromHours(24);

    /// <summary> 目前是否鎖定中 </summary>
    public bool IsLocked(string account, string clientIp)
    {
        var state = GetState(account, clientIp);
        return state.BanUntilUtc.HasValue && state.BanUntilUtc.Value > DateTime.UtcNow;
    }

    /// <summary> 紀錄一次失敗，達上限後鎖定 </summary>
    public void RegisterFailure(string account, string clientIp)
    {
        var key   = BuildKey(account, clientIp);
        var state = GetState(account, clientIp);
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

        _cacheService.SetCache(key, state, StateTtl);
    }

    /// <summary> 清除失敗紀錄（登入成功後呼叫） </summary>
    public void Reset(string account, string clientIp)
        => _cacheService.RemoveCache(BuildKey(account, clientIp));

    private LoginAttemptState GetState(string account, string clientIp)
        => _cacheService.GetCache<LoginAttemptState>(BuildKey(account, clientIp)) ?? new LoginAttemptState();

    private static string BuildKey(string account, string clientIp)
    {
        var safeAccount = string.IsNullOrWhiteSpace(account)
            ? "__anonymous"
            : account.Trim().ToLowerInvariant();
        var safeIp = string.IsNullOrWhiteSpace(clientIp) ? "unknown" : clientIp.Trim();
        return $"{CacheKeyPrefix}_{safeAccount}_{safeIp}";
    }

    private sealed class LoginAttemptState
    {
        public int FailCount { get; set; }
        public DateTime? BanUntilUtc { get; set; }
    }
}
