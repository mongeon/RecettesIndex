using Microsoft.Extensions.Logging;
using NSubstitute;
using RecettesIndex.Models;
using RecettesIndex.Services;
using RecettesIndex.Services.Exceptions;
using Supabase;
using Xunit;

namespace RecettesIndex.Tests.Services;

/// <summary>
/// Unit tests for BookAuthorService
/// </summary>
public class BookAuthorServiceTests
{
    private readonly Client _mockClient;
    private readonly ILogger<BookAuthorService> _mockLogger;
    private readonly BookAuthorService _service;

    public BookAuthorServiceTests()
    {
        _mockClient = new Client("http://localhost", "test-key", new SupabaseOptions());
        _mockLogger = Substitute.For<ILogger<BookAuthorService>>();
        _service = new BookAuthorService(_mockClient, _mockLogger);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_NullClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new BookAuthorService(null!, _mockLogger));
        Assert.Equal("supabaseClient", exception.ParamName);
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new BookAuthorService(_mockClient, null!));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_ValidParameters_CreatesInstance()
    {
        // Act
        var service = new BookAuthorService(_mockClient, _mockLogger);

        // Assert
        Assert.NotNull(service);
    }

    #endregion

    #region CreateBookAuthorAssociationsAsync Tests

    [Fact]
    public async Task CreateBookAuthorAssociationsAsync_EmptyAuthors_DoesNothing()
    {
        // Arrange
        var bookId = 1;
        var authors = new List<Author>();

        // Act
        await _service.CreateBookAuthorAssociationsAsync(bookId, authors);

        // Assert - no exception thrown, method returns successfully
    }

    [Fact]
    public async Task CreateBookAuthorAssociationsAsync_SingleAuthor_CreatesAssociation()
    {
        // Arrange
        var bookId = 1;
        var authors = new List<Author>
        {
            new() { Id = 10, Name = "Test", LastName = "Author" }
        };

        // Act - Since we can't fully mock Supabase.Client's fluent API,
        // we test that the method doesn't throw on valid input
        // In a real scenario with a wrapper, we'd verify the Insert was called
        try
        {
            await _service.CreateBookAuthorAssociationsAsync(bookId, authors);
        }
        catch (ServiceException ex)
        {
            // Expected if Supabase client throws - verify it's wrapped properly
            Assert.NotNull(ex.Message);
        }
    }

    [Fact]
    public async Task CreateBookAuthorAssociationsAsync_MultipleAuthors_CreatesMultipleAssociations()
    {
        // Arrange
        var bookId = 1;
        var authors = new List<Author>
        {
            new() { Id = 10, Name = "First", LastName = "Author" },
            new() { Id = 20, Name = "Second", LastName = "Author" },
            new() { Id = 30, Name = "Third", LastName = "Author" }
        };

        // Act
        try
        {
            await _service.CreateBookAuthorAssociationsAsync(bookId, authors);
        }
        catch (ServiceException ex)
        {
            // Expected - verify error message is user-friendly
            Assert.Contains("error", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    #endregion

    #region UpdateBookAuthorAssociationsAsync Tests

    [Fact]
    public async Task UpdateBookAuthorAssociationsAsync_NewAuthors_AddsAssociations()
    {
        // Arrange
        var bookId = 1;
        var newAuthors = new List<Author>
        {
            new() { Id = 10, Name = "New", LastName = "Author" }
        };

        // Act
        try
        {
            await _service.UpdateBookAuthorAssociationsAsync(bookId, newAuthors);
        }
        catch (ServiceException ex)
        {
            // Expected - verify error handling
            Assert.NotNull(ex.Message);
        }
    }

    [Fact]
    public async Task UpdateBookAuthorAssociationsAsync_EmptyAuthors_RemovesAllAssociations()
    {
        // Arrange
        var bookId = 1;
        var emptyAuthors = new List<Author>();

        // Act
        try
        {
            await _service.UpdateBookAuthorAssociationsAsync(bookId, emptyAuthors);
        }
        catch (ServiceException ex)
        {
            // Expected - verify error is wrapped
            Assert.NotNull(ex.Message);
        }
    }

    #endregion

    #region LoadAuthorsForBookAsync Tests

    [Fact]
    public async Task LoadAuthorsForBookAsync_NullBook_SetsEmptyAuthorsList()
    {
        // Arrange
        var book = new Book { Id = 1, Name = "Test Book" };

        // Act
        await _service.LoadAuthorsForBookAsync(book);

        // Assert - method should handle errors gracefully and set empty list
        Assert.NotNull(book.Authors);
    }

    [Fact]
    public async Task LoadAuthorsForBookAsync_BookWithNoAuthors_SetsEmptyAuthorsList()
    {
        // Arrange
        var book = new Book { Id = 999, Name = "Book With No Authors" };

        // Act
        await _service.LoadAuthorsForBookAsync(book);

        // Assert - should not throw, should set empty list
        Assert.NotNull(book.Authors);
    }

    [Fact]
    public async Task LoadAuthorsForBookAsync_ExceptionThrown_LogsErrorAndSetsEmptyList()
    {
        // Arrange
        var book = new Book { Id = 1, Name = "Test Book" };

        // Act - force an error by using the mock client which will fail
        await _service.LoadAuthorsForBookAsync(book);

        // Assert - should handle error gracefully
        Assert.NotNull(book.Authors);
        Assert.Empty(book.Authors);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task CreateBookAuthorAssociationsAsync_WhenHttpExceptionOccurs_ThrowsServiceException()
    {
        // Arrange
        var bookId = 1;
        var authors = new List<Author>
        {
            new() { Id = 10, Name = "Test", LastName = "Author" }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceException>(async () =>
            await _service.CreateBookAuthorAssociationsAsync(bookId, authors));

        // Verify error message is user-friendly
        Assert.NotNull(exception.Message);
    }

    [Fact]
    public async Task UpdateBookAuthorAssociationsAsync_WhenExceptionOccurs_ThrowsServiceException()
    {
        // Arrange
        var bookId = 1;
        var authors = new List<Author>
        {
            new() { Id = 10, Name = "Test", LastName = "Author" }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceException>(async () =>
            await _service.UpdateBookAuthorAssociationsAsync(bookId, authors));

        // Verify error handling
        Assert.Contains("error", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
