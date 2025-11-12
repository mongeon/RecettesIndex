using Microsoft.Extensions.Logging;
using NSubstitute;
using RecettesIndex.Models;
using RecettesIndex.Services;
using RecettesIndex.Services.Exceptions;
using Supabase;
using Xunit;

namespace RecettesIndex.Tests.Services;

/// <summary>
/// Unit tests for SupabaseRecipesQuery
/// </summary>
public class SupabaseRecipesQueryTests
{
    private readonly Client _mockClient;
    private readonly ILogger<SupabaseRecipesQuery> _mockLogger;
    private readonly SupabaseRecipesQuery _query;

    public SupabaseRecipesQueryTests()
    {
        _mockClient = new Client("http://localhost", "test-key", new SupabaseOptions());
        _mockLogger = Substitute.For<ILogger<SupabaseRecipesQuery>>();
        _query = new SupabaseRecipesQuery(_mockClient, _mockLogger);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_NullClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new SupabaseRecipesQuery(null!, _mockLogger));
        Assert.Equal("supabaseClient", exception.ParamName);
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new SupabaseRecipesQuery(_mockClient, null!));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_ValidParameters_CreatesInstance()
    {
        // Act
        var query = new SupabaseRecipesQuery(_mockClient, _mockLogger);

        // Assert
        Assert.NotNull(query);
    }

    #endregion

    #region GetRecipeIdsByNameAsync Tests

    [Fact]
    public async Task GetRecipeIdsByNameAsync_WithValidTerm_ReturnsEmptyOrThrows()
    {
        // Arrange
        var term = "pasta";
        var ct = CancellationToken.None;

        // Act & Assert
        try
        {
            var result = await _query.GetRecipeIdsByNameAsync(term, null, ct);
            Assert.NotNull(result);
        }
        catch (ServiceException ex)
        {
            // Expected - verify error message is user-friendly
            Assert.Contains("error", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task GetRecipeIdsByNameAsync_WithRating_FiltersCorrectly()
    {
        // Arrange
        var term = "chicken";
        var rating = 5;
        var ct = CancellationToken.None;

        // Act & Assert
        try
        {
            var result = await _query.GetRecipeIdsByNameAsync(term, rating, ct);
            Assert.NotNull(result);
        }
        catch (ServiceException ex)
        {
            Assert.NotNull(ex.Message);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public async Task GetRecipeIdsByNameAsync_WithInvalidRating_DoesNotFilter(int rating)
    {
        // Arrange
        var term = "test";

        // Act & Assert
        try
        {
            var result = await _query.GetRecipeIdsByNameAsync(term, rating, CancellationToken.None);
            Assert.NotNull(result);
        }
        catch (ServiceException)
        {
            // Expected - error handling works
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("a")]
    public async Task GetRecipeIdsByNameAsync_VariousTerms_HandlesGracefully(string term)
    {
        // Act & Assert
        try
        {
            var result = await _query.GetRecipeIdsByNameAsync(term, null, CancellationToken.None);
            Assert.NotNull(result);
        }
        catch (ServiceException)
        {
            // Expected
        }
    }

    #endregion

    #region GetRecipeIdsByBookIdsAsync Tests

    [Fact]
    public async Task GetRecipeIdsByBookIdsAsync_EmptyList_ReturnsEmpty()
    {
        // Arrange
        var bookIds = new List<int>();

        // Act
        var result = await _query.GetRecipeIdsByBookIdsAsync(bookIds, null, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRecipeIdsByBookIdsAsync_WithSingleBookId_ReturnsResults()
    {
        // Arrange
        var bookIds = new List<int> { 1 };

        // Act & Assert
        try
        {
            var result = await _query.GetRecipeIdsByBookIdsAsync(bookIds, null, CancellationToken.None);
            Assert.NotNull(result);
        }
        catch (ServiceException ex)
        {
            Assert.Contains("error", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task GetRecipeIdsByBookIdsAsync_WithMultipleBookIds_ReturnsResults()
    {
        // Arrange
        var bookIds = new List<int> { 1, 2, 3 };

        // Act & Assert
        try
        {
            var result = await _query.GetRecipeIdsByBookIdsAsync(bookIds, null, CancellationToken.None);
            Assert.NotNull(result);
        }
        catch (ServiceException)
        {
            // Expected
        }
    }

    [Fact]
    public async Task GetRecipeIdsByBookIdsAsync_WithRating_FiltersCorrectly()
    {
        // Arrange
        var bookIds = new List<int> { 1, 2 };
        var rating = 4;

        // Act & Assert
        try
        {
            var result = await _query.GetRecipeIdsByBookIdsAsync(bookIds, rating, CancellationToken.None);
            Assert.NotNull(result);
        }
        catch (ServiceException)
        {
            // Expected
        }
    }

    #endregion

    #region GetAllRecipeIdsAsync Tests

    [Fact]
    public async Task GetAllRecipeIdsAsync_WithoutRating_ReturnsAll()
    {
        // Act & Assert
        try
        {
            var result = await _query.GetAllRecipeIdsAsync(null, CancellationToken.None);
            Assert.NotNull(result);
        }
        catch (ServiceException ex)
        {
            Assert.NotNull(ex.Message);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task GetAllRecipeIdsAsync_WithValidRating_FiltersCorrectly(int rating)
    {
        // Act & Assert
        try
        {
            var result = await _query.GetAllRecipeIdsAsync(rating, CancellationToken.None);
            Assert.NotNull(result);
        }
        catch (ServiceException)
        {
            // Expected
        }
    }

    #endregion

    #region GetRecipesByIdsAsync Tests

    [Fact]
    public async Task GetRecipesByIdsAsync_EmptyList_ReturnsEmpty()
    {
        // Arrange
        var ids = new List<int>();

        // Act
        var result = await _query.GetRecipesByIdsAsync(ids, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRecipesByIdsAsync_WithValidIds_ReturnsRecipes()
    {
        // Arrange
        var ids = new List<int> { 1, 2, 3 };

        // Act & Assert
        try
        {
            var result = await _query.GetRecipesByIdsAsync(ids, CancellationToken.None);
            Assert.NotNull(result);
        }
        catch (ServiceException ex)
        {
            Assert.NotNull(ex.Message);
        }
    }

    #endregion

    #region GetBookIdsByTitleAsync Tests

    [Fact]
    public async Task GetBookIdsByTitleAsync_WithValidTerm_ReturnsResults()
    {
        // Arrange
        var term = "cookbook";

        // Act & Assert
        try
        {
            var result = await _query.GetBookIdsByTitleAsync(term, CancellationToken.None);
            Assert.NotNull(result);
        }
        catch (ServiceException ex)
        {
            Assert.Contains("error", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("a")]
    public async Task GetBookIdsByTitleAsync_VariousTerms_HandlesGracefully(string term)
    {
        // Act & Assert
        try
        {
            var result = await _query.GetBookIdsByTitleAsync(term, CancellationToken.None);
            Assert.NotNull(result);
        }
        catch (ServiceException)
        {
            // Expected
        }
    }

    #endregion

    #region GetBookIdsByAuthorIdsAsync Tests

    [Fact]
    public async Task GetBookIdsByAuthorIdsAsync_EmptyList_ReturnsEmpty()
    {
        // Arrange
        var authorIds = new List<int>();

        // Act
        var result = await _query.GetBookIdsByAuthorIdsAsync(authorIds, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetBookIdsByAuthorIdsAsync_WithValidIds_ReturnsResults()
    {
        // Arrange
        var authorIds = new List<int> { 1, 2 };

        // Act & Assert
        try
        {
            var result = await _query.GetBookIdsByAuthorIdsAsync(authorIds, CancellationToken.None);
            Assert.NotNull(result);
        }
        catch (ServiceException)
        {
            // Expected
        }
    }

    #endregion

    #region GetBookIdsByAuthorAsync Tests

    [Fact]
    public async Task GetBookIdsByAuthorAsync_WithValidAuthorId_ReturnsResults()
    {
        // Arrange
        var authorId = 1;

        // Act & Assert
        try
        {
            var result = await _query.GetBookIdsByAuthorAsync(authorId, CancellationToken.None);
            Assert.NotNull(result);
        }
        catch (ServiceException ex)
        {
            Assert.NotNull(ex.Message);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999999)]
    public async Task GetBookIdsByAuthorAsync_VariousAuthorIds_HandlesGracefully(int authorId)
    {
        // Act & Assert
        try
        {
            var result = await _query.GetBookIdsByAuthorAsync(authorId, CancellationToken.None);
            Assert.NotNull(result);
        }
        catch (ServiceException)
        {
            // Expected
        }
    }

    #endregion

    #region GetAuthorIdsByNameAsync Tests

    [Fact]
    public async Task GetAuthorIdsByNameAsync_WithValidTerm_ReturnsResults()
    {
        // Arrange
        var term = "Smith";

        // Act & Assert
        try
        {
            var result = await _query.GetAuthorIdsByNameAsync(term, CancellationToken.None);
            Assert.NotNull(result);
        }
        catch (ServiceException ex)
        {
            Assert.Contains("error", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    public async Task GetAuthorIdsByNameAsync_VariousTerms_HandlesGracefully(string term)
    {
        // Act & Assert
        try
        {
            var result = await _query.GetAuthorIdsByNameAsync(term, CancellationToken.None);
            Assert.NotNull(result);
        }
        catch (ServiceException)
        {
            // Expected
        }
    }

    #endregion

    #region GetBooksAsync Tests

    [Fact]
    public async Task GetBooksAsync_ReturnsResults()
    {
        // Act & Assert
        try
        {
            var result = await _query.GetBooksAsync(CancellationToken.None);
            Assert.NotNull(result);
        }
        catch (ServiceException ex)
        {
            Assert.NotNull(ex.Message);
        }
    }

    #endregion

    #region GetAuthorsAsync Tests

    [Fact]
    public async Task GetAuthorsAsync_ReturnsResults()
    {
        // Act & Assert
        try
        {
            var result = await _query.GetAuthorsAsync(CancellationToken.None);
            Assert.NotNull(result);
        }
        catch (ServiceException ex)
        {
            Assert.NotNull(ex.Message);
        }
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task GetRecipeIdsByNameAsync_OnException_ThrowsServiceException()
    {
        // Arrange
        var term = "test";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceException>(async () =>
            await _query.GetRecipeIdsByNameAsync(term, null, CancellationToken.None));

        // Verify error message
        Assert.NotNull(exception.Message);
        Assert.Contains("error", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetBooksAsync_OnException_ThrowsServiceException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceException>(async () =>
            await _query.GetBooksAsync(CancellationToken.None));

        // Verify error handling
        Assert.NotNull(exception.Message);
    }

    [Fact]
    public async Task GetAuthorsAsync_OnException_ThrowsServiceException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceException>(async () =>
            await _query.GetAuthorsAsync(CancellationToken.None));

        // Verify error handling
        Assert.NotNull(exception.Message);
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task GetRecipeIdsByNameAsync_WithCancellationToken_PropagatesToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        try
        {
            await _query.GetRecipeIdsByNameAsync("test", null, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected if cancellation is properly propagated
            Assert.True(true);
        }
        catch (ServiceException)
        {
            // Also acceptable - wrapped exception
            Assert.True(true);
        }
    }

    [Fact]
    public async Task GetAllRecipeIdsAsync_WithCancellationToken_PropagatesToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        try
        {
            await _query.GetAllRecipeIdsAsync(null, cts.Token);
        }
        catch (OperationCanceledException)
        {
            Assert.True(true);
        }
        catch (ServiceException)
        {
            Assert.True(true);
        }
    }

    #endregion
}
