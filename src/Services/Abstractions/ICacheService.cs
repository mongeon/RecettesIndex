namespace RecettesIndex.Services.Abstractions;

public interface ICacheService
{
    public Task<T> GetOrCreateAsync<T>(string key, TimeSpan ttl, Func<CancellationToken, Task<T>> factory, CancellationToken ct = default);
    public void Remove(string key);
}
