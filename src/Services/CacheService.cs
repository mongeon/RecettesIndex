using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using RecettesIndex.Services.Abstractions;

namespace RecettesIndex.Services;

/// <summary>
/// In-memory cache service with exception handling and type safety.
/// </summary>
public class CacheService(ILogger<CacheService>? logger = null) : ICacheService
{
    private class CacheEntry(object value, DateTimeOffset expiresAt)
    {
        public object Value { get; } = value;
        public DateTimeOffset ExpiresAt { get; } = expiresAt;
    }

    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly ILogger<CacheService>? _logger = logger;

    /// <summary>
    /// Gets or creates a cached value with comprehensive error handling.
    /// </summary>
    /// <typeparam name="T">The type of value to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="ttl">Time-to-live for the cache entry.</param>
    /// <param name="factory">Factory function to create the value if not cached.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The cached or newly created value.</returns>
    public async Task<T> GetOrCreateAsync<T>(string key, TimeSpan ttl, Func<CancellationToken, Task<T>> factory, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger?.LogWarning("Cache key is null or empty, calling factory directly");
            return await factory(ct);
        }

        try
        {
            // Try to get from cache
            if (_cache.TryGetValue(key, out var entry))
            {
                // Check if expired
                if (entry.ExpiresAt > DateTimeOffset.UtcNow)
                {
                    // Validate type before casting
                    if (entry.Value is T cachedValue)
                    {
                        _logger?.LogDebug("Cache hit for key: {Key}", key);
                        return cachedValue;
                    }
                    else
                    {
                        // Type mismatch - remove invalid entry
                        _logger?.LogWarning("Cache type mismatch for key {Key}. Expected {ExpectedType}, got {ActualType}. Removing entry.", 
                            key, typeof(T).Name, entry.Value?.GetType().Name ?? "null");
                        Remove(key);
                    }
                }
                else
                {
                    // Entry expired - remove it
                    _logger?.LogDebug("Cache entry expired for key: {Key}", key);
                    Remove(key);
                }
            }

            // Cache miss or invalid entry - call factory
            _logger?.LogDebug("Cache miss for key: {Key}, calling factory", key);
            var value = await factory(ct);
            
            // Only cache non-null values
            if (value != null)
            {
                try
                {
                    var expiresAt = DateTimeOffset.UtcNow.Add(ttl);
                    _cache[key] = new CacheEntry(value, expiresAt);
                    _logger?.LogDebug("Cached value for key: {Key}, expires at: {ExpiresAt}", key, expiresAt);
                }
                catch (Exception ex)
                {
                    // Failed to cache, but we still have the value
                    _logger?.LogWarning(ex, "Failed to cache value for key: {Key}", key);
                }
            }
            
            return value;
        }
        catch (OperationCanceledException)
        {
            _logger?.LogInformation("Cache operation cancelled for key: {Key}", key);
            throw;
        }
        catch (Exception ex)
        {
            // Log the error but try to get fresh data
            _logger?.LogError(ex, "Cache operation failed for key {Key}, attempting to call factory", key);
            
            try
            {
                // If cache fails, still try to get the value from factory
                return await factory(ct);
            }
            catch (Exception factoryEx)
            {
                _logger?.LogError(factoryEx, "Factory also failed for key {Key}", key);
                throw;
            }
        }
    }

    /// <summary>
    /// Removes a cache entry by key.
    /// </summary>
    /// <param name="key">The cache key to remove.</param>
    public void Remove(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        try
        {
            if (_cache.TryRemove(key, out _))
            {
                _logger?.LogDebug("Removed cache entry for key: {Key}", key);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to remove cache entry for key: {Key}", key);
        }
    }

    /// <summary>
    /// Clears all cache entries. Useful for testing or manual cache invalidation.
    /// </summary>
    public void Clear()
    {
        try
        {
            _cache.Clear();
            _logger?.LogInformation("Cache cleared");
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to clear cache");
        }
    }

    /// <summary>
    /// Gets the current number of cached entries.
    /// </summary>
    public int Count => _cache.Count;
}
