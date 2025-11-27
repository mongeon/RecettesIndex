using Microsoft.Extensions.Logging;
using RecettesIndex.Services.Abstractions;

namespace RecettesIndex.Services;

public static class CacheServiceExtensions
{
    public static void RemoveMany(this ICacheService cache, params string[] keys)
    {
        if (cache is null || keys is null || keys.Length == 0) return;
        foreach (var key in keys)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                cache.Remove(key);
            }
        }
    }

    public static Task<IReadOnlyList<T>> GetOrEmptyAsync<T>(
        this ICacheService cache,
        string key,
        TimeSpan ttl,
        Func<CancellationToken, Task<IReadOnlyList<T>>> factory,
        ILogger? logger = null,
        CancellationToken ct = default)
    {
        return cache.GetOrCreateAsync<IReadOnlyList<T>>(key, ttl, async token =>
        {
            try
            {
                var items = await factory(token);
                return items ?? Array.Empty<T>();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error loading cached list for key: {Key}", key);
                return Array.Empty<T>();
            }
        }, ct);
    }
}
