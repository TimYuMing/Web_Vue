using Microsoft.Extensions.Caching.Memory;

namespace Web_Vue.Server.Tools;

/// <summary> 記憶體快取工具 </summary>
public class CacheTool(IMemoryCache _memoryCache)
{
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromHours(24);

    /// <summary> AOP 方式：若快取存在直接回傳，否則執行委派後存入快取 </summary>
    public async Task<T> GetOrSetCacheAsync<T>(string key, Func<Task<T>> getData, TimeSpan? ttl = null)
    {
        if (_memoryCache.TryGetValue(key, out T cached))
        {
            return cached;
        }

        var result = await getData();
        _memoryCache.Set(key, result, ttl ?? DefaultTtl);
        return result;
    }

    /// <summary> 儲存快取 </summary>
    public void SetCache<T>(string key, T value, TimeSpan ttl)
        => _memoryCache.Set(key, value, ttl);

    /// <summary> 讀取快取 </summary>
    public T? GetCache<T>(string key)
        => _memoryCache.TryGetValue(key, out T value) ? value : default;

    /// <summary> 清除快取 </summary>
    public void RemoveCache(string key)
        => _memoryCache.Remove(key);

    /// <summary> 清除全部快取 </summary>
    public void ClearAllCache()
    {
        if (_memoryCache is MemoryCache mc)
        {
            mc.Clear();
        }
    }
}
