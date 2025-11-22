using System;
using System.Collections.Generic;
using System.Linq;
using RecettesIndex.Models;
using Xunit;

namespace RecettesIndex.Tests.Pages;

/// <summary>
/// Tests for dashboard statistics calculation logic.
/// </summary>
public class DashboardStatisticsTests
{
    [Fact]
    public void CalculateAverageRating_WithRatedRecipes_ReturnsCorrectAverage()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() { Id = 1, Name = "Recipe 1", Rating = 5, CreationDate = DateTime.UtcNow },
            new() { Id = 2, Name = "Recipe 2", Rating = 4, CreationDate = DateTime.UtcNow },
            new() { Id = 3, Name = "Recipe 3", Rating = 3, CreationDate = DateTime.UtcNow }
        };

        // Act
        var ratedRecipes = recipes.Where(r => r.Rating > 0).ToList();
        var average = ratedRecipes.Average(r => r.Rating);

        // Assert
        Assert.Equal(4.0, average, 1);
    }

    [Fact]
    public void CalculateAverageRating_WithNoRatedRecipes_ReturnsZero()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() { Id = 1, Name = "Recipe 1", Rating = 0, CreationDate = DateTime.UtcNow },
            new() { Id = 2, Name = "Recipe 2", Rating = 0, CreationDate = DateTime.UtcNow }
        };

        // Act
        var ratedRecipes = recipes.Where(r => r.Rating > 0).ToList();
        var average = ratedRecipes.Any() ? ratedRecipes.Average(r => r.Rating) : 0;

        // Assert
        Assert.Equal(0, average);
    }

    [Fact]
    public void CalculateRatingDistribution_GroupsRecipesByRating()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() { Id = 1, Name = "Recipe 1", Rating = 5, CreationDate = DateTime.UtcNow },
            new() { Id = 2, Name = "Recipe 2", Rating = 5, CreationDate = DateTime.UtcNow },
            new() { Id = 3, Name = "Recipe 3", Rating = 4, CreationDate = DateTime.UtcNow },
            new() { Id = 4, Name = "Recipe 4", Rating = 3, CreationDate = DateTime.UtcNow },
            new() { Id = 5, Name = "Recipe 5", Rating = 0, CreationDate = DateTime.UtcNow }
        };

        // Act
        var distribution = recipes
            .Where(r => r.Rating > 0)
            .GroupBy(r => r.Rating)
            .ToDictionary(g => g.Key, g => g.Count());

        // Assert
        Assert.Equal(3, distribution.Count); // 3 different ratings (5, 4, 3)
        Assert.Equal(2, distribution[5]); // 2 recipes with 5 stars
        Assert.Equal(1, distribution[4]); // 1 recipe with 4 stars
        Assert.Equal(1, distribution[3]); // 1 recipe with 3 stars
        Assert.False(distribution.ContainsKey(0)); // Unrated not included
    }

    [Fact]
    public void CountUnratedRecipes_ReturnsCorrectCount()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() { Id = 1, Name = "Recipe 1", Rating = 5, CreationDate = DateTime.UtcNow },
            new() { Id = 2, Name = "Recipe 2", Rating = 0, CreationDate = DateTime.UtcNow },
            new() { Id = 3, Name = "Recipe 3", Rating = 0, CreationDate = DateTime.UtcNow }
        };

        // Act
        var unratedCount = recipes.Count(r => r.Rating == 0);

        // Assert
        Assert.Equal(2, unratedCount);
    }

    [Fact]
    public void GetTopRatedRecipes_ReturnsHighestRatedInOrder()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var recipes = new List<Recipe>
        {
            new() { Id = 1, Name = "Recipe 1", Rating = 3, CreationDate = now.AddDays(-5) },
            new() { Id = 2, Name = "Recipe 2", Rating = 5, CreationDate = now.AddDays(-2) },
            new() { Id = 3, Name = "Recipe 3", Rating = 4, CreationDate = now.AddDays(-3) },
            new() { Id = 4, Name = "Recipe 4", Rating = 5, CreationDate = now.AddDays(-1) },
            new() { Id = 5, Name = "Recipe 5", Rating = 2, CreationDate = now.AddDays(-4) }
        };

        // Act
        var topRated = recipes
            .Where(r => r.Rating > 0)
            .OrderByDescending(r => r.Rating)
            .ThenByDescending(r => r.CreationDate)
            .Take(3)
            .ToList();

        // Assert
        Assert.Equal(3, topRated.Count);
        Assert.Equal(4, topRated[0].Id); // Most recent 5-star
        Assert.Equal(2, topRated[1].Id); // Older 5-star
        Assert.Equal(3, topRated[2].Id); // 4-star
    }

    [Fact]
    public void GetBookRecipeCounts_GroupsRecipesByBook()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() { Id = 1, Name = "Recipe 1", Rating = 5, BookId = 1, CreationDate = DateTime.UtcNow },
            new() { Id = 2, Name = "Recipe 2", Rating = 4, BookId = 1, CreationDate = DateTime.UtcNow },
            new() { Id = 3, Name = "Recipe 3", Rating = 3, BookId = 2, CreationDate = DateTime.UtcNow },
            new() { Id = 4, Name = "Recipe 4", Rating = 5, BookId = null, CreationDate = DateTime.UtcNow }
        };

        // Act
        var bookCounts = recipes
            .Where(r => r.BookId.HasValue)
            .GroupBy(r => r.BookId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        // Assert
        Assert.Equal(2, bookCounts.Count); // 2 books
        Assert.Equal(2, bookCounts[1]); // Book 1 has 2 recipes
        Assert.Equal(1, bookCounts[2]); // Book 2 has 1 recipe
        Assert.False(bookCounts.ContainsKey(0)); // Null book not included
    }

    [Fact]
    public void CountRecentRecipes_Last7Days_ReturnsCorrectCount()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var recipes = new List<Recipe>
        {
            new() { Id = 1, Name = "Recipe 1", Rating = 5, CreationDate = now.AddDays(-2) },
            new() { Id = 2, Name = "Recipe 2", Rating = 4, CreationDate = now.AddDays(-5) },
            new() { Id = 3, Name = "Recipe 3", Rating = 3, CreationDate = now.AddDays(-10) },
            new() { Id = 4, Name = "Recipe 4", Rating = 5, CreationDate = now.AddDays(-1) }
        };

        // Act
        var last7Days = recipes.Count(r => r.CreationDate >= now.AddDays(-7));

        // Assert
        Assert.Equal(3, last7Days); // 3 recipes in last 7 days
    }

    [Fact]
    public void CountRecentRecipes_Last30Days_ReturnsCorrectCount()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var recipes = new List<Recipe>
        {
            new() { Id = 1, Name = "Recipe 1", Rating = 5, CreationDate = now.AddDays(-2) },
            new() { Id = 2, Name = "Recipe 2", Rating = 4, CreationDate = now.AddDays(-15) },
            new() { Id = 3, Name = "Recipe 3", Rating = 3, CreationDate = now.AddDays(-40) },
            new() { Id = 4, Name = "Recipe 4", Rating = 5, CreationDate = now.AddDays(-25) }
        };

        // Act
        var last30Days = recipes.Count(r => r.CreationDate >= now.AddDays(-30));

        // Assert
        Assert.Equal(3, last30Days); // 3 recipes in last 30 days
    }

    [Fact]
    public void GetMostRecentRecipe_ReturnsNewestRecipe()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var recipes = new List<Recipe>
        {
            new() { Id = 1, Name = "Recipe 1", Rating = 5, CreationDate = now.AddDays(-5) },
            new() { Id = 2, Name = "Recipe 2", Rating = 4, CreationDate = now.AddDays(-2) },
            new() { Id = 3, Name = "Recipe 3", Rating = 3, CreationDate = now.AddDays(-10) }
        };

        // Act
        var mostRecent = recipes.OrderByDescending(r => r.CreationDate).FirstOrDefault();

        // Assert
        Assert.NotNull(mostRecent);
        Assert.Equal(2, mostRecent.Id);
        Assert.Equal("Recipe 2", mostRecent.Name);
    }

    [Fact]
    public void CalculatePercentage_WithValidData_ReturnsCorrectPercentage()
    {
        // Arrange
        int count = 25;
        int total = 100;

        // Act
        var percentage = total > 0 ? (count * 100.0 / total) : 0;

        // Assert
        Assert.Equal(25.0, percentage);
    }

    [Fact]
    public void CalculatePercentage_WithZeroTotal_ReturnsZero()
    {
        // Arrange
        int count = 25;
        int total = 0;

        // Act
        var percentage = total > 0 ? (count * 100.0 / total) : 0;

        // Assert
        Assert.Equal(0, percentage);
    }

    [Theory]
    [InlineData(5, 10)]
    [InlineData(4, 8)]
    [InlineData(3, 5)]
    public void FilterByRating_ReturnsOnlyMatchingRecipes(int targetRating, int expectedCount)
    {
        // Arrange
        var recipes = new List<Recipe>();
        for (int i = 1; i <= 10; i++)
        {
            recipes.Add(new Recipe
            {
                Id = i,
                Name = $"Recipe {i}",
                Rating = 5,
                CreationDate = DateTime.UtcNow
            });
        }
        for (int i = 11; i <= 18; i++)
        {
            recipes.Add(new Recipe
            {
                Id = i,
                Name = $"Recipe {i}",
                Rating = 4,
                CreationDate = DateTime.UtcNow
            });
        }
        for (int i = 19; i <= 23; i++)
        {
            recipes.Add(new Recipe
            {
                Id = i,
                Name = $"Recipe {i}",
                Rating = 3,
                CreationDate = DateTime.UtcNow
            });
        }

        // Act
        var filtered = recipes.Where(r => r.Rating == targetRating).ToList();

        // Assert
        Assert.Equal(expectedCount, filtered.Count);
        Assert.All(filtered, r => Assert.Equal(targetRating, r.Rating));
    }

    [Fact]
    public void FilterByRating_FourPlusStars_ReturnsCorrectRecipes()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() { Id = 1, Name = "Recipe 1", Rating = 5, CreationDate = DateTime.UtcNow },
            new() { Id = 2, Name = "Recipe 2", Rating = 4, CreationDate = DateTime.UtcNow },
            new() { Id = 3, Name = "Recipe 3", Rating = 3, CreationDate = DateTime.UtcNow },
            new() { Id = 4, Name = "Recipe 4", Rating = 5, CreationDate = DateTime.UtcNow }
        };

        // Act
        var fourPlusStars = recipes.Where(r => r.Rating >= 4).ToList();

        // Assert
        Assert.Equal(3, fourPlusStars.Count);
        Assert.All(fourPlusStars, r => Assert.True(r.Rating >= 4));
    }
}
