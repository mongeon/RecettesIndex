namespace RecettesIndex.Services.Abstractions;

public interface ICacheService
{
    Task<T> GetOrCreateAsync<T>(string key, TimeSpan ttl, Func<CancellationToken, Task<T>> factory, CancellationToken ct = default);
    void Remove(string key);
}
