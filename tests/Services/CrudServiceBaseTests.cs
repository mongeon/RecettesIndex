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

    public Task<Result<string>> GetByIdCoreAsync_Public(int id, Func<Task<string?>> fetchSingle, string notFoundMessage, string logContext, string unexpectedUserMessage)
        => GetByIdCoreAsync(id, fetchSingle, notFoundMessage, logContext, unexpectedUserMessage);

    public Task<Result<string>> CreateCoreAsync_Public(string entity,
        Func<string?> validate,
        Func<Task<string?>> doInsert,
        Action<string>? onSuccess,
        string unexpectedUserMessage)
        => CreateCoreAsync(entity, validate, doInsert, onSuccess, unexpectedUserMessage);

    public Task<Result<string>> UpdateCoreAsync_Public(string entity,
        Func<string?> validate,
        Func<Task<string?>> doUpdate,
        Action<string>? onSuccess,
        string unexpectedUserMessage,
        int? idForLogging = null,
        string? entityNameForLogging = null)
        => UpdateCoreAsync(entity, validate, doUpdate, onSuccess, unexpectedUserMessage, idForLogging, entityNameForLogging);

    public Task<Result<bool>> DeleteCoreAsync_Public(int id,
        Func<Task<string?>> getExisting,
        Func<Task> doDelete,
        Action? onSuccess,
        string notFoundMessage,
        string unexpectedUserMessage,
        string entityNameForLogging)
        => DeleteCoreAsync(id, getExisting, doDelete, onSuccess, notFoundMessage, unexpectedUserMessage, entityNameForLogging);
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
        var result = await _service.GetByIdCoreAsync_Public(42, () => Task.FromResult<string?>(null), "not found", "get", "unexpected");
        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetByIdCoreAsync_HttpRequestException_ReturnsNetworkFailure()
    {
        var result = await _service.GetByIdCoreAsync_Public(42, () => throw new HttpRequestException("net"), "not found", "get", "unexpected");
        Assert.False(result.IsSuccess);
        Assert.Contains("network", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetByIdCoreAsync_Success_ReturnsValue()
    {
        var result = await _service.GetByIdCoreAsync_Public(7, () => Task.FromResult<string?>("ok"), "not found", "get", "unexpected");
        Assert.True(result.IsSuccess);
        Assert.Equal("ok", result.Value);
    }

    [Fact]
    public async Task CreateCoreAsync_ValidationFailure_ReturnsFailure()
    {
        var result = await _service.CreateCoreAsync_Public("x", () => "invalid input", () => Task.FromResult<string?>("x"), _ => { }, "unexpected");
        Assert.False(result.IsSuccess);
        Assert.Contains("invalid", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateCoreAsync_InsertNull_ReturnsFailure()
    {
        var result = await _service.CreateCoreAsync_Public("x", () => null, () => Task.FromResult<string?>(null), _ => { }, "unexpected");
        Assert.False(result.IsSuccess);
        Assert.Contains("failed", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateCoreAsync_Success_InvokesOnSuccess()
    {
        var called = false;
        var result = await _service.CreateCoreAsync_Public("x", () => null, () => Task.FromResult<string?>("created"), _ => called = true, "unexpected");
        Assert.True(result.IsSuccess);
        Assert.Equal("created", result.Value);
        Assert.True(called);
    }

    [Fact]
    public async Task UpdateCoreAsync_ValidationFailure_ReturnsFailure()
    {
        var result = await _service.UpdateCoreAsync_Public("x", () => "bad", () => Task.FromResult<string?>("x"), _ => { }, "unexpected", 1, "entity");
        Assert.False(result.IsSuccess);
        Assert.Contains("bad", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateCoreAsync_UpdateNull_ReturnsFailure()
    {
        var result = await _service.UpdateCoreAsync_Public("x", () => null, () => Task.FromResult<string?>(null), _ => { }, "unexpected", 1, "entity");
        Assert.False(result.IsSuccess);
        Assert.Contains("failed", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeleteCoreAsync_InvalidId_ReturnsFailure()
    {
        var result = await _service.DeleteCoreAsync_Public(0, () => Task.FromResult<string?>("existing"), () => Task.CompletedTask, () => { }, "not found", "unexpected", "entity");
        Assert.False(result.IsSuccess);
        Assert.Contains("invalid", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeleteCoreAsync_NotFound_ReturnsFailure()
    {
        var result = await _service.DeleteCoreAsync_Public(9, () => Task.FromResult<string?>(null), () => Task.CompletedTask, () => { }, "not found", "unexpected", "entity");
        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeleteCoreAsync_Success_ReturnsTrue()
    {
        var called = false;
        var result = await _service.DeleteCoreAsync_Public(9, () => Task.FromResult<string?>("x"), () => Task.CompletedTask, () => called = true, "not found", "unexpected", "entity");
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.True(called);
    }
}
