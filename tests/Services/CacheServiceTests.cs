using Microsoft.Extensions.Logging;
using NSubstitute;
using RecettesIndex.Services;
using RecettesIndex.Services.Abstractions;
using Xunit;

namespace RecettesIndex.Tests.Services;

public class CacheServiceTests
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheService> _logger;

    public CacheServiceTests()
    {
        _logger = Substitute.For<ILogger<CacheService>>();
        _cacheService = new CacheService(_logger);
    }

    [Fact]
    public async Task GetOrCreateAsync_CacheMiss_CallsFactory()
    {
        // Arrange
        var key = "test-key";
        var ttl = TimeSpan.FromMinutes(5);
        var factoryCalled = false;
        Func<CancellationToken, Task<string>> factory = ct =>
        {
            factoryCalled = true;
            return Task.FromResult("factory-value");
        };

        // Act
        var result = await _cacheService.GetOrCreateAsync(key, ttl, factory);

        // Assert
        Assert.Equal("factory-value", result);
        Assert.True(factoryCalled);
    }

    [Fact]
    public async Task GetOrCreateAsync_CacheHit_ReturnsValue()
    {
        // Arrange
        var key = "test-key";
        var ttl = TimeSpan.FromMinutes(5);
        var factoryCallCount = 0;
        Func<CancellationToken, Task<string>> factory = ct =>
        {
            factoryCallCount++;
            return Task.FromResult("factory-value");
        };

        // Act - First call to populate cache
        var result1 = await _cacheService.GetOrCreateAsync(key, ttl, factory);
        // Second call should hit cache
        var result2 = await _cacheService.GetOrCreateAsync(key, ttl, factory);

        // Assert
        Assert.Equal("factory-value", result1);
        Assert.Equal("factory-value", result2);
        Assert.Equal(1, factoryCallCount); // Factory should only be called once
    }

    [Fact]
    public async Task GetOrCreateAsync_ExpiredCache_CallsFactoryAgain()
    {
        // Arrange
        var key = "test-key";
        var ttl = TimeSpan.FromMilliseconds(100); // Very short TTL
        var factoryCallCount = 0;
        Func<CancellationToken, Task<string>> factory = ct =>
        {
            factoryCallCount++;
            return Task.FromResult($"value-{factoryCallCount}");
        };

        // Act
        var result1 = await _cacheService.GetOrCreateAsync(key, ttl, factory);
        await Task.Delay(150); // Wait for cache to expire
        var result2 = await _cacheService.GetOrCreateAsync(key, ttl, factory);

        // Assert
        Assert.Equal("value-1", result1);
        Assert.Equal("value-2", result2);
        Assert.Equal(2, factoryCallCount); // Factory called twice due to expiration
    }

    [Fact]
    public async Task GetOrCreateAsync_TypeMismatch_RemovesInvalidEntry()
    {
        // Arrange
        var key = "test-key";
        var ttl = TimeSpan.FromMinutes(5);

        // First, cache a string
        await _cacheService.GetOrCreateAsync(key, ttl, ct => Task.FromResult("string-value"));

        var factoryCalled = false;
        // Then try to get an int (type mismatch)
        Func<CancellationToken, Task<int>> intFactory = ct =>
        {
            factoryCalled = true;
            return Task.FromResult(42);
        };

        // Act
        var result = await _cacheService.GetOrCreateAsync(key, ttl, intFactory);

        // Assert
        Assert.Equal(42, result);
        Assert.True(factoryCalled); // Factory should be called since cached type doesn't match
    }

    [Fact]
    public async Task GetOrCreateAsync_NullKey_CallsFactoryDirectly()
    {
        // Arrange
        string? key = null;
        var ttl = TimeSpan.FromMinutes(5);
        var factoryCalled = false;
        Func<CancellationToken, Task<string>> factory = ct =>
        {
            factoryCalled = true;
            return Task.FromResult("value");
        };

        // Act
        var result = await _cacheService.GetOrCreateAsync(key!, ttl, factory);

        // Assert
        Assert.Equal("value", result);
        Assert.True(factoryCalled);
    }

    [Fact]
    public async Task GetOrCreateAsync_EmptyKey_CallsFactoryDirectly()
    {
        // Arrange
        var key = "";
        var ttl = TimeSpan.FromMinutes(5);
        var factoryCalled = false;
        Func<CancellationToken, Task<string>> factory = ct =>
        {
            factoryCalled = true;
            return Task.FromResult("value");
        };

        // Act
        var result = await _cacheService.GetOrCreateAsync(key, ttl, factory);

        // Assert
        Assert.Equal("value", result);
        Assert.True(factoryCalled);
    }

    [Fact]
    public async Task GetOrCreateAsync_FactoryThrowsException_PropagatesException()
    {
        // Arrange
        var key = "test-key";
        var ttl = TimeSpan.FromMinutes(5);
        Func<CancellationToken, Task<string>> factory = ct =>
        {
            throw new InvalidOperationException("Factory error");
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _cacheService.GetOrCreateAsync(key, ttl, factory));
    }

    [Fact]
    public async Task GetOrCreateAsync_NullValue_DoesNotCache()
    {
        // Arrange
        var key = "test-key";
        var ttl = TimeSpan.FromMinutes(5);
        var factoryCallCount = 0;
        Func<CancellationToken, Task<string?>> factory = ct =>
        {
            factoryCallCount++;
            return Task.FromResult<string?>(null);
        };

        // Act
        var result1 = await _cacheService.GetOrCreateAsync(key, ttl, factory);
        var result2 = await _cacheService.GetOrCreateAsync(key, ttl, factory);

        // Assert
        Assert.Null(result1);
        Assert.Null(result2);
        Assert.Equal(2, factoryCallCount); // Factory called twice since null is not cached
    }

    [Fact]
    public async Task Remove_ExistingKey_RemovesFromCache()
    {
        // Arrange
        var key = "test-key";
        var ttl = TimeSpan.FromMinutes(5);
        Func<CancellationToken, Task<string>> factory = ct => Task.FromResult("value");

        // First, populate cache
        await _cacheService.GetOrCreateAsync(key, ttl, factory);

        // Act
        _cacheService.Remove(key);

        // Assert - Factory should be called again after remove
        var factoryCalled = false;
        Func<CancellationToken, Task<string>> factoryCheck = ct =>
        {
            factoryCalled = true;
            return Task.FromResult("new-value");
        };
        await _cacheService.GetOrCreateAsync(key, ttl, factoryCheck);
        Assert.True(factoryCalled);
    }

    [Fact]
    public void Remove_NonExistentKey_DoesNotThrow()
    {
        // Arrange
        var key = "non-existent-key";

        // Act & Assert - Should not throw
        _cacheService.Remove(key);
    }

    [Fact]
    public void Remove_NullKey_DoesNotThrow()
    {
        // Arrange
        string? key = null;

        // Act & Assert - Should not throw
        _cacheService.Remove(key!);
    }

    [Fact]
    public async Task GetOrCreateAsync_CancellationToken_PassedToFactory()
    {
        // Arrange
        var key = "test-key";
        var ttl = TimeSpan.FromMinutes(5);
        var cts = new CancellationTokenSource();
        CancellationToken receivedToken = default;

        Func<CancellationToken, Task<string>> factory = ct =>
        {
            receivedToken = ct;
            return Task.FromResult("value");
        };

        // Act
        await _cacheService.GetOrCreateAsync(key, ttl, factory, cts.Token);

        // Assert
        Assert.Equal(cts.Token, receivedToken);
    }

    [Fact]
    public async Task GetOrCreateAsync_MultipleCalls_DifferentKeys_IndependentCache()
    {
        // Arrange
        var key1 = "key1";
        var key2 = "key2";
        var ttl = TimeSpan.FromMinutes(5);

        // Act
        var result1 = await _cacheService.GetOrCreateAsync(key1, ttl, ct => Task.FromResult("value1"));
        var result2 = await _cacheService.GetOrCreateAsync(key2, ttl, ct => Task.FromResult("value2"));

        // Assert
        Assert.Equal("value1", result1);
        Assert.Equal("value2", result2);
        // Verify they're independent
        _cacheService.Remove(key1);
        var result1Again = await _cacheService.GetOrCreateAsync(key1, ttl, ct => Task.FromResult("new-value1"));
        var result2Again = await _cacheService.GetOrCreateAsync(key2, ttl, ct => Task.FromResult("should-not-be-called"));
        Assert.Equal("new-value1", result1Again);
        Assert.Equal("value2", result2Again); // Still cached
    }
}
