using RecettesIndex.Services;
using Xunit;

namespace RecettesIndex.Tests.Services;

/// <summary>
/// Unit tests for service constants
/// </summary>
public class ServiceConstantsTests
{
    #region CacheConstants Tests

    [Fact]
    public void CacheConstants_DefaultTtl_IsThreeMinutes()
    {
        // Arrange & Act
        var ttl = CacheConstants.DefaultTtl;

        // Assert
        Assert.Equal(TimeSpan.FromMinutes(3), ttl);
        Assert.Equal(180, ttl.TotalSeconds);
    }

    [Fact]
    public void CacheConstants_BooksListKey_HasCorrectValue()
    {
        // Assert
        Assert.Equal("books:list", CacheConstants.BooksListKey);
    }

    [Fact]
    public void CacheConstants_AuthorsListKey_HasCorrectValue()
    {
        // Assert
        Assert.Equal("authors:list", CacheConstants.AuthorsListKey);
    }

    [Fact]
    public void CacheConstants_Keys_AreUnique()
    {
        // Arrange
        var keys = new[]
        {
            CacheConstants.BooksListKey,
            CacheConstants.AuthorsListKey
        };

        // Assert
        Assert.Equal(keys.Length, keys.Distinct().Count());
    }

    [Fact]
    public void CacheConstants_Keys_FollowNamingConvention()
    {
        // Assert - all keys should use colon separator
        Assert.Contains(":", CacheConstants.BooksListKey);
        Assert.Contains(":", CacheConstants.AuthorsListKey);
    }

    #endregion

    #region PaginationConstants Tests

    [Fact]
    public void PaginationConstants_MinPageSize_IsOne()
    {
        // Assert
        Assert.Equal(1, PaginationConstants.MinPageSize);
    }

    [Fact]
    public void PaginationConstants_MaxPageSize_IsOneHundred()
    {
        // Assert
        Assert.Equal(100, PaginationConstants.MaxPageSize);
    }

    [Fact]
    public void PaginationConstants_DefaultPageSize_IsTwenty()
    {
        // Assert
        Assert.Equal(20, PaginationConstants.DefaultPageSize);
    }

    [Fact]
    public void PaginationConstants_DefaultPageSize_IsWithinMinMaxRange()
    {
        // Assert
        Assert.True(PaginationConstants.DefaultPageSize >= PaginationConstants.MinPageSize);
        Assert.True(PaginationConstants.DefaultPageSize <= PaginationConstants.MaxPageSize);
    }

    [Fact]
    public void PaginationConstants_MinPageSize_IsLessThanMaxPageSize()
    {
        // Assert
        Assert.True(PaginationConstants.MinPageSize < PaginationConstants.MaxPageSize);
    }

    [Fact]
    public void PaginationConstants_PageSizes_ArePositive()
    {
        // Assert
        Assert.True(PaginationConstants.MinPageSize > 0);
        Assert.True(PaginationConstants.MaxPageSize > 0);
        Assert.True(PaginationConstants.DefaultPageSize > 0);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    [InlineData(100)]
    public void PaginationConstants_CommonPageSizes_AreWithinRange(int pageSize)
    {
        // Assert
        Assert.True(pageSize >= PaginationConstants.MinPageSize);
        Assert.True(pageSize <= PaginationConstants.MaxPageSize);
    }

    #endregion

    #region RecipeSortConstants Tests

    [Fact]
    public void RecipeSortConstants_Name_HasCorrectValue()
    {
        // Assert
        Assert.Equal("name", RecipeSortConstants.Name);
    }

    [Fact]
    public void RecipeSortConstants_Rating_HasCorrectValue()
    {
        // Assert
        Assert.Equal("rating", RecipeSortConstants.Rating);
    }

    [Fact]
    public void RecipeSortConstants_CreatedAt_HasCorrectValue()
    {
        // Assert
        Assert.Equal("created_at", RecipeSortConstants.CreatedAt);
    }

    [Fact]
    public void RecipeSortConstants_AllValues_AreUnique()
    {
        // Arrange
        var sortFields = new[]
        {
            RecipeSortConstants.Name,
            RecipeSortConstants.Rating,
            RecipeSortConstants.CreatedAt
        };

        // Assert
        Assert.Equal(sortFields.Length, sortFields.Distinct().Count());
    }

    [Fact]
    public void RecipeSortConstants_AllValues_AreNotEmpty()
    {
        // Assert
        Assert.False(string.IsNullOrWhiteSpace(RecipeSortConstants.Name));
        Assert.False(string.IsNullOrWhiteSpace(RecipeSortConstants.Rating));
        Assert.False(string.IsNullOrWhiteSpace(RecipeSortConstants.CreatedAt));
    }

    [Fact]
    public void RecipeSortConstants_AllValues_UseLowerSnakeCase()
    {
        // Assert - verify naming convention
        Assert.Equal(RecipeSortConstants.Name, RecipeSortConstants.Name.ToLowerInvariant());
        Assert.Equal(RecipeSortConstants.Rating, RecipeSortConstants.Rating.ToLowerInvariant());
        Assert.Equal(RecipeSortConstants.CreatedAt, RecipeSortConstants.CreatedAt.ToLowerInvariant());
    }

    [Fact]
    public void RecipeSortConstants_CreatedAt_UsesSnakeCase()
    {
        // Assert - verify database column naming convention
        Assert.Contains("_", RecipeSortConstants.CreatedAt);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void ServiceConstants_AllConstantsAreAccessible()
    {
        // Act - verify all constants can be accessed without errors
        var cacheTtl = CacheConstants.DefaultTtl;
        var booksKey = CacheConstants.BooksListKey;
        var authorsKey = CacheConstants.AuthorsListKey;
        var minPage = PaginationConstants.MinPageSize;
        var maxPage = PaginationConstants.MaxPageSize;
        var defaultPage = PaginationConstants.DefaultPageSize;
        var sortName = RecipeSortConstants.Name;
        var sortRating = RecipeSortConstants.Rating;
        var sortCreated = RecipeSortConstants.CreatedAt;

        // Assert - all values are not null/default
        Assert.NotEqual(TimeSpan.Zero, cacheTtl);
        Assert.NotNull(booksKey);
        Assert.NotNull(authorsKey);
        Assert.NotEqual(0, minPage);
        Assert.NotEqual(0, maxPage);
        Assert.NotEqual(0, defaultPage);
        Assert.NotNull(sortName);
        Assert.NotNull(sortRating);
        Assert.NotNull(sortCreated);
    }

    #endregion
}
