using Supabase.Postgrest.Models;

namespace RecettesIndex.Services.Abstractions;

public record DataResult<T>(IReadOnlyList<T> Models, int? Count);

public interface IDataApi
{
    public ITableQuery<T> From<T>() where T : BaseModel, new();
}

public interface ITableQuery<T> where T : BaseModel, new()
{
    public ITableQuery<T> Filter(string column, string op, object value);
    public ITableQuery<T> Range(int from, int to);
    public Task<DataResult<T>> Get(Supabase.Postgrest.QueryOptions? options = null, CancellationToken ct = default);
    public Task<DataResult<T>> Insert(T model, CancellationToken ct = default);
    public Task<DataResult<T>> Update(T model, CancellationToken ct = default);
    public Task Delete(CancellationToken ct = default);
}
