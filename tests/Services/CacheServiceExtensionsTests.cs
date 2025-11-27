using Microsoft.Extensions.Logging;
using NSubstitute;
using RecettesIndex.Services;
using RecettesIndex.Services.Abstractions;
using Xunit;

namespace RecettesIndex.Tests.Services;

public class CacheServiceExtensionsTests
{
    [Fact]
    public void RemoveMany_RemovesAllNonEmptyKeys()
    {
        // Arrange
        var logger = Substitute.For<ILogger<CacheService>>();
        var cache = new CacheService(logger);
        cache.Remove("missing");

        // Act & Assert - should not throw
        cache.RemoveMany("a", " ", null!, "b", "");
    }

    [Fact]
    public async Task GetOrEmptyAsync_Error_ReturnsEmptyList()
    {
        // Arrange
        var logger = Substitute.For<ILogger<CacheService>>();
        var cache = new CacheService(logger);

        // Act
        var result = await cache.GetOrEmptyAsync<int>(
            "key",
            TimeSpan.FromSeconds(1),
            ct => throw new InvalidOperationException("boom"),
            logger);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetOrEmptyAsync_Success_ReturnsFactoryList()
    {
        var cache = new CacheService(Substitute.For<ILogger<CacheService>>());
        var items = await cache.GetOrEmptyAsync<int>(
            "key2",
            TimeSpan.FromSeconds(1),
            ct => Task.FromResult<IReadOnlyList<int>>(new List<int> { 1, 2, 3 }));
        
        Assert.Equal(3, items.Count);
        Assert.Contains(2, items);
    }
}
