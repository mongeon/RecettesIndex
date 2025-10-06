using Supabase.Postgrest.Models;

namespace RecettesIndex.Services.Abstractions;

public record DataResult<T>(IReadOnlyList<T> Models, int? Count);

public interface IDataApi
{
    ITableQuery<T> From<T>() where T : BaseModel, new();
}

public interface ITableQuery<T> where T : BaseModel, new()
{
    ITableQuery<T> Filter(string column, string op, object value);
    ITableQuery<T> Range(int from, int to);
    Task<DataResult<T>> Get(Supabase.Postgrest.QueryOptions? options = null, CancellationToken ct = default);
    Task<DataResult<T>> Insert(T model, CancellationToken ct = default);
    Task<DataResult<T>> Update(T model, CancellationToken ct = default);
    Task Delete(CancellationToken ct = default);
}
