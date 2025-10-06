using System.Collections.Concurrent;
using RecettesIndex.Services.Abstractions;

namespace RecettesIndex.Services;

public class CacheService : ICacheService
{
    private class CacheEntry(object value, DateTimeOffset expiresAt)
    {
        public object Value { get; } = value;
        public DateTimeOffset ExpiresAt { get; } = expiresAt;
    }

    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();

    public async Task<T> GetOrCreateAsync<T>(string key, TimeSpan ttl, Func<CancellationToken, Task<T>> factory, CancellationToken ct = default)
    {
        if (_cache.TryGetValue(key, out var entry) && entry.ExpiresAt > DateTimeOffset.UtcNow)
        {
            return (T)entry.Value;
        }

        var value = await factory(ct);
        var expiresAt = DateTimeOffset.UtcNow.Add(ttl);
        _cache[key] = new CacheEntry(value!, expiresAt);
        return value;
    }

    public void Remove(string key)
    {
        _cache.TryRemove(key, out _);
    }
}
