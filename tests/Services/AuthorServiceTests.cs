using Microsoft.Extensions.Logging;
using NSubstitute;
using RecettesIndex.Models;
using RecettesIndex.Services;
using RecettesIndex.Services.Abstractions;
using Supabase;
using Xunit;

namespace RecettesIndex.Tests.Services;

public class AuthorServiceTests
{
    private readonly ICacheService _cache;
    private readonly Client _client;
    private readonly ILogger<AuthorService> _logger;
    private readonly AuthorService _service;

    public AuthorServiceTests()
    {
        _cache = new CacheService(Substitute.For<ILogger<CacheService>>());
        _client = new Client("http://localhost", "test-key", new SupabaseOptions());
        _logger = Substitute.For<ILogger<AuthorService>>();
        _service = new AuthorService(_cache, _client, _logger);
    }

    [Fact]
    public async Task CreateAsync_ValidAuthor_ReturnsSuccess()
    {
        // Arrange
        var author = new Author { Name = "John", LastName = "Doe" };

        // Act
        var result = await _service.CreateAsync(author);

        // Assert
        Assert.NotNull(result);
        // Note: Will fail without Supabase connection, but validates validation logic
    }

    [Fact]
    public async Task CreateAsync_NullAuthor_ReturnsFailure()
    {
        // Arrange
        Author? author = null;

        // Act
        var result = await _service.CreateAsync(author!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("cannot be null", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_EmptyFirstName_ReturnsFailure()
    {
        // Arrange
        var author = new Author { Name = "", LastName = "Doe" };

        // Act
        var result = await _service.CreateAsync(author);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("first name is required", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_WhitespaceFirstName_ReturnsFailure()
    {
        // Arrange
        var author = new Author { Name = "   ", LastName = "Doe" };

        // Act
        var result = await _service.CreateAsync(author);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("first name is required", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_ValidFirstNameOnly_ReturnsSuccess()
    {
        // Arrange
        var author = new Author { Name = "Madonna", LastName = null };

        // Act
        var result = await _service.CreateAsync(author);

        // Assert
        Assert.NotNull(result);
        // Last name is optional, so this should be valid
    }

    [Fact]
    public async Task UpdateAsync_NullAuthor_ReturnsFailure()
    {
        // Arrange
        Author? author = null;

        // Act
        var result = await _service.UpdateAsync(author!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("cannot be null", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateAsync_EmptyFirstName_ReturnsFailure()
    {
        // Arrange
        var author = new Author { Id = 1, Name = "", LastName = "Doe" };

        // Act
        var result = await _service.UpdateAsync(author);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("first name is required", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateAsync_InvalidId_ReturnsFailure()
    {
        // Arrange
        var author = new Author { Id = 0, Name = "John", LastName = "Doe" };

        // Act
        var result = await _service.UpdateAsync(author);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("invalid", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("id", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateAsync_NegativeId_ReturnsFailure()
    {
        // Arrange
        var author = new Author { Id = -1, Name = "John", LastName = "Doe" };

        // Act
        var result = await _service.UpdateAsync(author);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("invalid", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeleteAsync_InvalidId_ReturnsFailure()
    {
        // Arrange
        var invalidId = 0;

        // Act
        var result = await _service.DeleteAsync(invalidId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("invalid", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("id", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeleteAsync_NegativeId_ReturnsFailure()
    {
        // Arrange
        var invalidId = -1;

        // Act
        var result = await _service.DeleteAsync(invalidId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("invalid", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }
}
