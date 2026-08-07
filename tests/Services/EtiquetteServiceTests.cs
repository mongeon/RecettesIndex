using Microsoft.Extensions.Logging;
using NSubstitute;
using RecettesIndex.Services;
using RecettesIndex.Services.Abstractions;
using Supabase;
using Xunit;

namespace RecettesIndex.Tests.Services;

/// <summary>
/// Covers the behaviour reachable without a Supabase connection: the guards, the
/// French-facing messages, and the caching contract. The client points at a dead
/// address, exactly as in the other service tests, so anything past validation
/// fails at the network rather than reaching a database.
/// </summary>
public class EtiquetteServiceTests
{
    private readonly ICacheService _cache;
    private readonly Client _client;
    private readonly ILogger<EtiquetteService> _logger;
    private readonly EtiquetteService _service;

    public EtiquetteServiceTests()
    {
        var cacheLogger = Substitute.For<ILogger<CacheService>>();
        _cache = new CacheService(cacheLogger);
        _client = new Client("http://localhost", "test-key", new SupabaseOptions());
        _logger = Substitute.For<ILogger<EtiquetteService>>();
        _service = new EtiquetteService(_cache, _client, _logger);
    }

    [Fact]
    public void EtiquetteService_Constructor_ThrowsWhenCacheIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new EtiquetteService(null!, _client, _logger));
    }

    [Fact]
    public void EtiquetteService_Constructor_ThrowsWhenClientIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new EtiquetteService(_cache, null!, _logger));
    }

    [Fact]
    public void EtiquetteService_Constructor_ThrowsWhenLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new EtiquetteService(_cache, _client, null!));
    }

    [Fact]
    public void EtiquetteService_ImplementsIEtiquetteService()
    {
        Assert.IsAssignableFrom<IEtiquetteService>(_service);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GetOrCreateAsync_BlankName_ReturnsFailure(string? name)
    {
        // Act
        var result = await _service.GetOrCreateAsync(name!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Le nom de l'étiquette est requis", result.ErrorMessage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SetForRecipeAsync_InvalidRecipeId_ReturnsFrenchFailure(int recipeId)
    {
        // Act
        var result = await _service.SetForRecipeAsync(recipeId, new[] { 1, 2 });

        // Assert — this message can surface in the UI, so it must not be the English
        // one the shared validation guard would produce.
        Assert.False(result.IsSuccess);
        Assert.Equal("Identifiant de recette invalide", result.ErrorMessage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task DeleteAsync_InvalidId_ReturnsFailure(int id)
    {
        // Act
        var result = await _service.DeleteAsync(id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task GetAllAsync_CachesTheList()
    {
        // Act
        var first = await _service.GetAllAsync();
        var second = await _service.GetAllAsync();

        // Assert — the second call must be served from the cache, since the tag picker
        // and the filter bar both read this list on every render.
        Assert.NotNull(first);
        Assert.Same(first, second);
    }

    [Fact]
    public async Task GetOrCreateAsync_BlankName_LeavesCacheIntact()
    {
        // Arrange
        var cached = await _service.GetAllAsync();

        // Act
        await _service.GetOrCreateAsync("   ");

        // Assert — a rejected call must not evict the list every consumer is holding.
        Assert.Same(cached, await _service.GetAllAsync());
    }
}
