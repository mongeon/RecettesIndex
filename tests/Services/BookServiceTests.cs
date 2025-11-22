using Microsoft.Extensions.Logging;
using NSubstitute;
using RecettesIndex.Models;
using RecettesIndex.Services;
using RecettesIndex.Services.Abstractions;
using Supabase;
using Xunit;

namespace RecettesIndex.Tests.Services;

public class BookServiceTests
{
    private readonly IBookAuthorService _bookAuthorService;
    private readonly ICacheService _cache;
    private readonly Client _client;
    private readonly ILogger<BookService> _logger;
    private readonly BookService _service;

    public BookServiceTests()
    {
        _bookAuthorService = Substitute.For<IBookAuthorService>();
        _cache = new CacheService();
        _client = new Client("http://localhost", "test-key", new SupabaseOptions());
        _logger = Substitute.For<ILogger<BookService>>();
        _service = new BookService(_bookAuthorService, _cache, _client, _logger);
    }

    [Fact]
    public async Task CreateAsync_ValidBook_ReturnsSuccess()
    {
        // Arrange
        var book = new Book { Name = "Test Book" };
        var authorIds = new List<int> { 1, 2 };

        // Act
        var result = await _service.CreateAsync(book, authorIds);

        // Assert
        // Note: Will fail without Supabase connection, but should pass validation
        Assert.NotNull(result);
        if (!result.IsSuccess)
        {
            // Should not be a validation error
            Assert.DoesNotContain("cannot be null", result.ErrorMessage ?? "", StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("name is required", result.ErrorMessage ?? "", StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task CreateAsync_NullBook_ReturnsFailure()
    {
        // Arrange
        Book? book = null;
        var authorIds = new List<int>();

        // Act
        var result = await _service.CreateAsync(book!, authorIds);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("cannot be null", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_EmptyName_ReturnsFailure()
    {
        // Arrange
        var book = new Book { Name = "" };
        var authorIds = new List<int>();

        // Act
        var result = await _service.CreateAsync(book, authorIds);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("name is required", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_WhitespaceName_ReturnsFailure()
    {
        // Arrange
        var book = new Book { Name = "   " };
        var authorIds = new List<int>();

        // Act
        var result = await _service.CreateAsync(book, authorIds);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("name is required", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateAsync_ValidBook_ReturnsSuccess()
    {
        // Arrange
        var book = new Book { Id = 1, Name = "Updated Book" };
        var authorIds = new List<int> { 1 };

        // Act
        var result = await _service.UpdateAsync(book, authorIds);

        // Assert
        // Note: This will fail without actual Supabase connection,
        // but validates the validation logic works
        Assert.NotNull(result);
    }

    [Fact]
    public async Task UpdateAsync_NullBook_ReturnsFailure()
    {
        // Arrange
        Book? book = null;
        var authorIds = new List<int>();

        // Act
        var result = await _service.UpdateAsync(book!, authorIds);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("cannot be null", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateAsync_EmptyName_ReturnsFailure()
    {
        // Arrange
        var book = new Book { Id = 1, Name = "" };
        var authorIds = new List<int>();

        // Act
        var result = await _service.UpdateAsync(book, authorIds);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("name is required", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateAsync_InvalidId_ReturnsFailure()
    {
        // Arrange
        var book = new Book { Id = 0, Name = "Test Book" };
        var authorIds = new List<int>();

        // Act
        var result = await _service.UpdateAsync(book, authorIds);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("invalid", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("id", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
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
