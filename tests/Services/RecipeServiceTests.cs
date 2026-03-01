using System.Threading;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RecettesIndex.Models;
using RecettesIndex.Services;
using RecettesIndex.Services.Abstractions;
using Supabase;
using Xunit;

namespace RecettesIndex.Tests.Services;

public class RecipeServiceTests
{
    private readonly IRecipesQuery _query;
    private readonly ICacheService _cache;
    private readonly Client _client;
    private readonly ILogger<RecipeService> _logger;
    private readonly RecipeService _service;

    public RecipeServiceTests()
    {
        _query = Substitute.For<IRecipesQuery>();
        _cache = new CacheService(Substitute.For<ILogger<CacheService>>());
        _client = new Client("http://localhost", "test-key", new SupabaseOptions());
        _logger = Substitute.For<ILogger<RecipeService>>();
        _service = new RecipeService(_query, _cache, _client, _logger);
    }

    #region CreateAsync Validation Tests

    [Fact]
    public async Task CreateAsync_NullRecipe_ReturnsFailure()
    {
        // Arrange
        Recipe? recipe = null;

        // Act
        var result = await _service.CreateAsync(recipe!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("cannot be null", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_EmptyName_ReturnsFailure()
    {
        // Arrange
        var recipe = new Recipe { Name = "", Rating = 3 };

        // Act
        var result = await _service.CreateAsync(recipe);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("name is required", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_WhitespaceName_ReturnsFailure()
    {
        // Arrange
        var recipe = new Recipe { Name = "   ", Rating = 3 };

        // Act
        var result = await _service.CreateAsync(recipe);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("name is required", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_NegativeRating_ReturnsFailure()
    {
        // Arrange
        var recipe = new Recipe { Name = "Test Recipe", Rating = -1 };

        // Act
        var result = await _service.CreateAsync(recipe);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("rating", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("0", result.ErrorMessage);
        Assert.Contains("5", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateAsync_RatingAboveFive_ReturnsFailure()
    {
        // Arrange
        var recipe = new Recipe { Name = "Test Recipe", Rating = 6 };

        // Act
        var result = await _service.CreateAsync(recipe);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("rating", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("0", result.ErrorMessage);
        Assert.Contains("5", result.ErrorMessage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task CreateAsync_ValidRatings_DoesNotFailValidation(int rating)
    {
        // Arrange
        var recipe = new Recipe { Name = "Test Recipe", Rating = rating };

        // Act
        var result = await _service.CreateAsync(recipe);

        // Assert
        // Note: Will fail due to no Supabase connection, but should pass validation
        Assert.NotNull(result);
        if (!result.IsSuccess)
        {
            // Should not be a validation error
            Assert.DoesNotContain("rating", result.ErrorMessage ?? "", StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("name is required", result.ErrorMessage ?? "", StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task CreateAsync_NegativeBookPage_ReturnsFailure()
    {
        // Arrange
        var recipe = new Recipe { Name = "Test Recipe", Rating = 3, BookPage = -5 };

        // Act
        var result = await _service.CreateAsync(recipe);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("book page", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("positive", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_ZeroBookPage_ReturnsFailure()
    {
        // Arrange
        var recipe = new Recipe { Name = "Test Recipe", Rating = 3, BookPage = 0 };

        // Act
        var result = await _service.CreateAsync(recipe);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("book page", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("positive", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_ValidBookPage_DoesNotFailValidation()
    {
        // Arrange
        var recipe = new Recipe { Name = "Test Recipe", Rating = 3, BookPage = 42 };

        // Act
        var result = await _service.CreateAsync(recipe);

        // Assert
        Assert.NotNull(result);
        if (!result.IsSuccess)
        {
            // Should not be a validation error
            Assert.DoesNotContain("book page", result.ErrorMessage ?? "", StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task CreateAsync_NullBookPage_IsValid()
    {
        // Arrange
        var recipe = new Recipe { Name = "Test Recipe", Rating = 3, BookPage = null };

        // Act
        var result = await _service.CreateAsync(recipe);

        // Assert
        Assert.NotNull(result);
        if (!result.IsSuccess)
        {
            // Should not be a validation error
            Assert.DoesNotContain("book page", result.ErrorMessage ?? "", StringComparison.OrdinalIgnoreCase);
        }
    }

    #endregion

    #region UpdateAsync Validation Tests

    [Fact]
    public async Task UpdateAsync_NullRecipe_ReturnsFailure()
    {
        // Arrange
        Recipe? recipe = null;

        // Act
        var result = await _service.UpdateAsync(recipe!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("cannot be null", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateAsync_InvalidId_ReturnsFailure()
    {
        // Arrange
        var recipe = new Recipe { Id = 0, Name = "Test Recipe", Rating = 3 };

        // Act
        var result = await _service.UpdateAsync(recipe);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("invalid", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("id", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateAsync_NegativeId_ReturnsFailure()
    {
        // Arrange
        var recipe = new Recipe { Id = -1, Name = "Test Recipe", Rating = 3 };

        // Act
        var result = await _service.UpdateAsync(recipe);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("invalid", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("id", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateAsync_EmptyName_ReturnsFailure()
    {
        // Arrange
        var recipe = new Recipe { Id = 1, Name = "", Rating = 3 };

        // Act
        var result = await _service.UpdateAsync(recipe);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("name is required", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateAsync_InvalidRating_ReturnsFailure()
    {
        // Arrange
        var recipe = new Recipe { Id = 1, Name = "Test Recipe", Rating = 10 };

        // Act
        var result = await _service.UpdateAsync(recipe);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("rating", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateAsync_NegativeBookPage_ReturnsFailure()
    {
        // Arrange
        var recipe = new Recipe { Id = 1, Name = "Test Recipe", Rating = 3, BookPage = -1 };

        // Act
        var result = await _service.UpdateAsync(recipe);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("book page", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("positive", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region DeleteAsync Validation Tests

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
        Assert.Contains("id", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ValidId_CallsQuery()
    {
        // Arrange
        var expected = new Recipe { Id = 1, Name = "Test Recipe" };
        _query.GetRecipesByIdsAsync(Arg.Is<IReadOnlyCollection<int>>(ids => ids.Contains(1)), Arg.Any<CancellationToken>(), Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(new List<Recipe> { expected });

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(expected.Id, result.Value.Id);
        Assert.Equal(expected.Name, result.Value.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsFailure()
    {
        // Arrange
        _query.GetRecipesByIdsAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>(), Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(new List<Recipe>());

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region GetRecipeSummariesAsync Tests

    [Fact]
    public async Task GetRecipeSummariesAsync_WhenQuerySucceeds_ReturnsSuccess()
    {
        // Arrange
        var summaries = new List<Recipe>
        {
            new() { Id = 1, Rating = 5 },
            new() { Id = 2, Rating = 3 }
        };
        _query.GetRecipeSummariesAsync(null, Arg.Any<CancellationToken>())
            .Returns(summaries);

        // Act
        var result = await _service.GetRecipeSummariesAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
    }

    [Fact]
    public async Task GetRecipeSummariesAsync_WhenQueryThrows_ReturnsFailure()
    {
        // Arrange
        _query.GetRecipeSummariesAsync(null, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<IReadOnlyList<Recipe>>(new Exception("DB error")));

        // Act
        var result = await _service.GetRecipeSummariesAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
    }

    #endregion

    #region GetRecipesByIdsAsync Tests

    [Fact]
    public async Task GetRecipesByIdsAsync_WhenQuerySucceeds_ReturnsSuccess()
    {
        // Arrange
        var ids = new List<int> { 1, 2 };
        var recipes = new List<Recipe>
        {
            new() { Id = 1, Name = "Pasta" },
            new() { Id = 2, Name = "Salad" }
        };
        _query.GetRecipesByIdsAsync(Arg.Is<IReadOnlyCollection<int>>(c => c.Count == 2), Arg.Any<CancellationToken>(), null, false, 0, 0)
            .Returns(recipes);

        // Act
        var result = await _service.GetRecipesByIdsAsync(ids);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
    }

    [Fact]
    public async Task GetRecipesByIdsAsync_WhenQueryThrows_ReturnsFailure()
    {
        // Arrange
        var ids = new List<int> { 1 };
        _query.GetRecipesByIdsAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>(), null, false, 0, 0)
            .Returns(Task.FromException<IReadOnlyList<Recipe>>(new Exception("DB error")));

        // Act
        var result = await _service.GetRecipesByIdsAsync(ids);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
    }

    #endregion
}
