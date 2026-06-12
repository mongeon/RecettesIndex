using Microsoft.Extensions.Logging;
using NSubstitute;
using RecettesIndex.Services;
using RecettesIndex.Services.Abstractions;
using Supabase;
using Xunit;

namespace RecettesIndex.Tests.Services;

public class FakeCrudServiceString(ICacheService cache, Client client, ILogger<FakeCrudServiceString> logger)
    : CrudServiceBase<string, FakeCrudServiceString>(cache, client, logger)
{
    public Task<IReadOnlyList<string>> GetAllCachedAsync_Public(string key, Func<CancellationToken, Task<IReadOnlyList<string>>> fetch, CancellationToken ct = default)
        => GetAllCachedAsync(key, fetch, ct);

    public Task<Result<string>> GetByIdCoreAsync_Public(int id, Func<CancellationToken, Task<string?>> fetchSingle, string notFoundMessage, string logContext, string unexpectedUserMessage, CancellationToken ct = default)
        => GetByIdCoreAsync(id, fetchSingle, notFoundMessage, logContext, unexpectedUserMessage, ct);

    public Task<Result<string>> CreateCoreAsync_Public(string entity,
        Func<string?> validate,
        Func<CancellationToken, Task<string?>> doInsert,
        Action<string>? onSuccess,
        string unexpectedUserMessage,
        CancellationToken ct = default)
        => CreateCoreAsync(entity, validate, doInsert, onSuccess, unexpectedUserMessage, ct);

    public Task<Result<string>> UpdateCoreAsync_Public(string entity,
        Func<string?> validate,
        Func<CancellationToken, Task<string?>> doUpdate,
        Action<string>? onSuccess,
        string unexpectedUserMessage,
        int? idForLogging = null,
        string? entityNameForLogging = null,
        CancellationToken ct = default)
        => UpdateCoreAsync(entity, validate, doUpdate, onSuccess, unexpectedUserMessage, idForLogging, entityNameForLogging, ct);

    public Task<Result<bool>> DeleteCoreAsync_Public(int id,
        Func<CancellationToken, Task<string?>> getExisting,
        Func<CancellationToken, Task> doDelete,
        Action? onSuccess,
        string notFoundMessage,
        string unexpectedUserMessage,
        string entityNameForLogging,
        CancellationToken ct = default)
        => DeleteCoreAsync(id, getExisting, doDelete, onSuccess, notFoundMessage, unexpectedUserMessage, entityNameForLogging, ct);
}

public class CrudServiceBaseTests
{
    private readonly ICacheService _cache;
    private readonly Client _client;
    private readonly ILogger<FakeCrudServiceString> _logger;
    private readonly FakeCrudServiceString _service;

    public CrudServiceBaseTests()
    {
        _cache = new CacheService(Substitute.For<ILogger<CacheService>>());
        _client = new Client("http://localhost", "test-key", new SupabaseOptions());
        _logger = Substitute.For<ILogger<FakeCrudServiceString>>();
        _service = new FakeCrudServiceString(_cache, _client, _logger);
    }

    [Fact]
    public async Task GetAllCachedAsync_CachesAndReturns_List()
    {
        var key = "crud:test:list";
        var calls = 0;
        Func<CancellationToken, Task<IReadOnlyList<string>>> factory = ct =>
        {
            calls++;
            return Task.FromResult<IReadOnlyList<string>>(new List<string> { "a", "b", "c" });
        };

        var first = await _service.GetAllCachedAsync_Public(key, factory);
        var second = await _service.GetAllCachedAsync_Public(key, factory);

        Assert.Equal(new[] { "a", "b", "c" }, first);
        Assert.Equal(new[] { "a", "b", "c" }, second);
        Assert.Equal(1, calls); // cached on second call
    }

    [Fact]
    public async Task GetByIdCoreAsync_NotFound_ReturnsFailure()
    {
        var result = await _service.GetByIdCoreAsync_Public(42, _ => Task.FromResult<string?>(null), "not found", "get", "unexpected");
        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetByIdCoreAsync_HttpRequestException_ReturnsNetworkFailure()
    {
        var result = await _service.GetByIdCoreAsync_Public(42, _ => throw new HttpRequestException("net"), "not found", "get", "unexpected");
        Assert.False(result.IsSuccess);
        Assert.Contains("network", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetByIdCoreAsync_Success_ReturnsValue()
    {
        var result = await _service.GetByIdCoreAsync_Public(7, _ => Task.FromResult<string?>("ok"), "not found", "get", "unexpected");
        Assert.True(result.IsSuccess);
        Assert.Equal("ok", result.Value);
    }

    [Fact]
    public async Task CreateCoreAsync_ValidationFailure_ReturnsFailure()
    {
        var result = await _service.CreateCoreAsync_Public("x", () => "invalid input", _ => Task.FromResult<string?>("x"), _ => { }, "unexpected");
        Assert.False(result.IsSuccess);
        Assert.Contains("invalid", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateCoreAsync_InsertNull_ReturnsFailure()
    {
        var result = await _service.CreateCoreAsync_Public("x", () => null, _ => Task.FromResult<string?>(null), _ => { }, "unexpected");
        Assert.False(result.IsSuccess);
        Assert.Contains("failed", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateCoreAsync_Success_InvokesOnSuccess()
    {
        var called = false;
        var result = await _service.CreateCoreAsync_Public("x", () => null, _ => Task.FromResult<string?>("created"), _ => called = true, "unexpected");
        Assert.True(result.IsSuccess);
        Assert.Equal("created", result.Value);
        Assert.True(called);
    }

    [Fact]
    public async Task UpdateCoreAsync_ValidationFailure_ReturnsFailure()
    {
        var result = await _service.UpdateCoreAsync_Public("x", () => "bad", _ => Task.FromResult<string?>("x"), _ => { }, "unexpected", 1, "entity");
        Assert.False(result.IsSuccess);
        Assert.Contains("bad", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateCoreAsync_UpdateNull_ReturnsFailure()
    {
        var result = await _service.UpdateCoreAsync_Public("x", () => null, _ => Task.FromResult<string?>(null), _ => { }, "unexpected", 1, "entity");
        Assert.False(result.IsSuccess);
        Assert.Contains("failed", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeleteCoreAsync_InvalidId_ReturnsFailure()
    {
        var result = await _service.DeleteCoreAsync_Public(0, _ => Task.FromResult<string?>("existing"), _ => Task.CompletedTask, () => { }, "not found", "unexpected", "entity");
        Assert.False(result.IsSuccess);
        Assert.Contains("invalid", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeleteCoreAsync_NotFound_ReturnsFailure()
    {
        var result = await _service.DeleteCoreAsync_Public(9, _ => Task.FromResult<string?>(null), _ => Task.CompletedTask, () => { }, "not found", "unexpected", "entity");
        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeleteCoreAsync_Success_ReturnsTrue()
    {
        var called = false;
        var result = await _service.DeleteCoreAsync_Public(9, _ => Task.FromResult<string?>("x"), _ => Task.CompletedTask, () => called = true, "not found", "unexpected", "entity");
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.True(called);
    }

    [Fact]
    public async Task GetByIdCoreAsync_PassesCancellationTokenToFetch()
    {
        using var cts = new CancellationTokenSource();
        CancellationToken received = default;

        await _service.GetByIdCoreAsync_Public(1, ct =>
        {
            received = ct;
            return Task.FromResult<string?>("ok");
        }, "not found", "get", "unexpected", cts.Token);

        Assert.Equal(cts.Token, received);
    }

    [Fact]
    public async Task GetByIdCoreAsync_Cancelled_PropagatesOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _service.GetByIdCoreAsync_Public(1, ct =>
            {
                ct.ThrowIfCancellationRequested();
                return Task.FromResult<string?>("ok");
            }, "not found", "get", "unexpected", cts.Token));
    }

    [Fact]
    public async Task CreateCoreAsync_Cancelled_PropagatesOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _service.CreateCoreAsync_Public("x", () => null, ct =>
            {
                ct.ThrowIfCancellationRequested();
                return Task.FromResult<string?>("created");
            }, _ => { }, "unexpected", cts.Token));
    }
}
