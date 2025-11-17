using Xunit;
using RecettesIndex.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RecettesIndex.Tests.Pages;

/// <summary>
/// Tests for enhanced recipe filtering logic.
/// </summary>
public class RecipeFilteringTests
{
    [Fact]
    public void FilterRecipesBySearchTerm_CaseInsensitive_ReturnsMatches()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() { Id = 1, Name = "Chocolate Cake", Rating = 5, CreationDate = DateTime.UtcNow },
            new() { Id = 2, Name = "Vanilla Ice Cream", Rating = 4, CreationDate = DateTime.UtcNow },
            new() { Id = 3, Name = "Chocolate Cookies", Rating = 3, CreationDate = DateTime.UtcNow }
        };
        var searchTerm = "chocolate";

        // Act
        var filtered = recipes.Where(r => 
            r.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();

        // Assert
        Assert.Equal(2, filtered.Count);
        Assert.Contains(filtered, r => r.Name == "Chocolate Cake");
        Assert.Contains(filtered, r => r.Name == "Chocolate Cookies");
    }

    [Fact]
    public void FilterRecipesByRating_ReturnsOnlyMatchingRating()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() { Id = 1, Name = "Recipe 1", Rating = 5, CreationDate = DateTime.UtcNow },
            new() { Id = 2, Name = "Recipe 2", Rating = 4, CreationDate = DateTime.UtcNow },
            new() { Id = 3, Name = "Recipe 3", Rating = 5, CreationDate = DateTime.UtcNow }
        };
        int targetRating = 5;

        // Act
        var filtered = recipes.Where(r => r.Rating == targetRating).ToList();

        // Assert
        Assert.Equal(2, filtered.Count);
        Assert.All(filtered, r => Assert.Equal(5, r.Rating));
    }

    [Fact]
    public void FilterRecipesByBook_ReturnsOnlyRecipesFromBook()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() { Id = 1, Name = "Recipe 1", Rating = 5, BookId = 1, CreationDate = DateTime.UtcNow },
            new() { Id = 2, Name = "Recipe 2", Rating = 4, BookId = 2, CreationDate = DateTime.UtcNow },
            new() { Id = 3, Name = "Recipe 3", Rating = 5, BookId = 1, CreationDate = DateTime.UtcNow },
            new() { Id = 4, Name = "Recipe 4", Rating = 3, BookId = null, CreationDate = DateTime.UtcNow }
        };
        int targetBookId = 1;

        // Act
        var filtered = recipes.Where(r => r.BookId == targetBookId).ToList();

        // Assert
        Assert.Equal(2, filtered.Count);
        Assert.All(filtered, r => Assert.Equal(1, r.BookId));
    }

    [Fact]
    public void FilterUnratedRecipes_ReturnsOnlyUnrated()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() { Id = 1, Name = "Recipe 1", Rating = 5, CreationDate = DateTime.UtcNow },
            new() { Id = 2, Name = "Recipe 2", Rating = 0, CreationDate = DateTime.UtcNow },
            new() { Id = 3, Name = "Recipe 3", Rating = 0, CreationDate = DateTime.UtcNow }
        };

        // Act
        var unrated = recipes.Where(r => r.Rating == 0 || r.Rating < 1).ToList();

        // Assert
        Assert.Equal(2, unrated.Count);
        Assert.All(unrated, r => Assert.True(r.Rating == 0 || r.Rating < 1));
    }

    [Fact]
    public void CombinedFilters_SearchAndRating_ReturnsIntersection()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() { Id = 1, Name = "Chocolate Cake", Rating = 5, CreationDate = DateTime.UtcNow },
            new() { Id = 2, Name = "Chocolate Ice Cream", Rating = 4, CreationDate = DateTime.UtcNow },
            new() { Id = 3, Name = "Vanilla Cake", Rating = 5, CreationDate = DateTime.UtcNow }
        };
        var searchTerm = "chocolate";
        int targetRating = 5;

        // Act
        var filtered = recipes
            .Where(r => r.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .Where(r => r.Rating == targetRating)
            .ToList();

        // Assert
        Assert.Single(filtered);
        Assert.Equal("Chocolate Cake", filtered[0].Name);
        Assert.Equal(5, filtered[0].Rating);
    }

    [Fact]
    public void GetRecipeCountForBook_ReturnsCorrectCount()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() { Id = 1, Name = "Recipe 1", Rating = 5, BookId = 1, CreationDate = DateTime.UtcNow },
            new() { Id = 2, Name = "Recipe 2", Rating = 4, BookId = 1, CreationDate = DateTime.UtcNow },
            new() { Id = 3, Name = "Recipe 3", Rating = 5, BookId = 2, CreationDate = DateTime.UtcNow }
        };
        int targetBookId = 1;

        // Act
        var count = recipes.Count(r => r.BookId == targetBookId);

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public void GetRecipeCountForBook_NoRecipes_ReturnsZero()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() { Id = 1, Name = "Recipe 1", Rating = 5, BookId = 1, CreationDate = DateTime.UtcNow }
        };
        int targetBookId = 999;

        // Act
        var count = recipes.Count(r => r.BookId == targetBookId);

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public void HasActiveFilters_WithSearchTerm_ReturnsTrue()
    {
        // Arrange
        string searchTerm = "test";
        string? ratingFilter = null;
        string? bookFilter = null;
        string? authorFilter = null;

        // Act
        var hasFilters = !string.IsNullOrWhiteSpace(searchTerm) 
            || (ratingFilter != null && ratingFilter != "all") 
            || (bookFilter != null && bookFilter != "all") 
            || (authorFilter != null && authorFilter != "all");

        // Assert
        Assert.True(hasFilters);
    }

    [Fact]
    public void HasActiveFilters_NoFilters_ReturnsFalse()
    {
        // Arrange
        string searchTerm = string.Empty;
        string? ratingFilter = null;
        string? bookFilter = null;
        string? authorFilter = null;

        // Act
        var hasFilters = !string.IsNullOrWhiteSpace(searchTerm) 
            || (ratingFilter != null && ratingFilter != "all") 
            || (bookFilter != null && bookFilter != "all") 
            || (authorFilter != null && authorFilter != "all");

        // Assert
        Assert.False(hasFilters);
    }

    [Theory]
    [InlineData("5", true)]
    [InlineData("all", false)]
    [InlineData(null, false)]
    public void RatingFilter_IsActive_ReturnsExpectedResult(string? ratingFilter, bool expected)
    {
        // Arrange & Act
        var isActive = ratingFilter != null && ratingFilter != "all";

        // Assert
        Assert.Equal(expected, isActive);
    }

    [Fact]
    public void NotesPreview_LongText_TruncatesCorrectly()
    {
        // Arrange
        var longNotes = new string('x', 100);
        var maxLength = 50;

        // Act
        var preview = longNotes.Length > maxLength 
            ? longNotes.Substring(0, maxLength) + "..." 
            : longNotes;

        // Assert
        Assert.Equal(53, preview.Length); // 50 chars + "..."
        Assert.EndsWith("...", preview);
    }

    [Fact]
    public void NotesPreview_ShortText_DoesNotTruncate()
    {
        // Arrange
        var shortNotes = "Short notes";
        var maxLength = 50;

        // Act
        var preview = shortNotes.Length > maxLength 
            ? shortNotes.Substring(0, maxLength) + "..." 
            : shortNotes;

        // Assert
        Assert.Equal(shortNotes, preview);
        Assert.DoesNotContain("...", preview);
    }

    [Fact]
    public void SortRecipesByName_Ascending_ReturnsSortedList()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() { Id = 1, Name = "Zebra Cake", Rating = 5, CreationDate = DateTime.UtcNow },
            new() { Id = 2, Name = "Apple Pie", Rating = 4, CreationDate = DateTime.UtcNow },
            new() { Id = 3, Name = "Mango Smoothie", Rating = 5, CreationDate = DateTime.UtcNow }
        };

        // Act
        var sorted = recipes.OrderBy(r => r.Name).ToList();

        // Assert
        Assert.Equal("Apple Pie", sorted[0].Name);
        Assert.Equal("Mango Smoothie", sorted[1].Name);
        Assert.Equal("Zebra Cake", sorted[2].Name);
    }

    [Fact]
    public void SortRecipesByRating_Descending_ReturnsSortedList()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() { Id = 1, Name = "Recipe 1", Rating = 3, CreationDate = DateTime.UtcNow },
            new() { Id = 2, Name = "Recipe 2", Rating = 5, CreationDate = DateTime.UtcNow },
            new() { Id = 3, Name = "Recipe 3", Rating = 4, CreationDate = DateTime.UtcNow }
        };

        // Act
        var sorted = recipes.OrderByDescending(r => r.Rating).ToList();

        // Assert
        Assert.Equal(5, sorted[0].Rating);
        Assert.Equal(4, sorted[1].Rating);
        Assert.Equal(3, sorted[2].Rating);
    }

    [Fact]
    public void SortRecipesByDate_Descending_ReturnsSortedList()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var recipes = new List<Recipe>
        {
            new() { Id = 1, Name = "Recipe 1", Rating = 5, CreationDate = now.AddDays(-5) },
            new() { Id = 2, Name = "Recipe 2", Rating = 4, CreationDate = now.AddDays(-1) },
            new() { Id = 3, Name = "Recipe 3", Rating = 5, CreationDate = now.AddDays(-10) }
        };

        // Act
        var sorted = recipes.OrderByDescending(r => r.CreationDate).ToList();

        // Assert
        Assert.Equal(2, sorted[0].Id); // Most recent
        Assert.Equal(1, sorted[1].Id);
        Assert.Equal(3, sorted[2].Id); // Oldest
    }

    [Fact]
    public void FilterRecipesByAuthor_ThroughBooks_ReturnsCorrectRecipes()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() { Id = 1, Name = "Recipe 1", Rating = 5, BookId = 1, CreationDate = DateTime.UtcNow },
            new() { Id = 2, Name = "Recipe 2", Rating = 4, BookId = 2, CreationDate = DateTime.UtcNow },
            new() { Id = 3, Name = "Recipe 3", Rating = 5, BookId = 3, CreationDate = DateTime.UtcNow }
        };
        
        // Simulate books from a specific author
        var authorBookIds = new HashSet<int> { 1, 3 };

        // Act
        var filtered = recipes.Where(r => r.BookId.HasValue && authorBookIds.Contains(r.BookId.Value)).ToList();

        // Assert
        Assert.Equal(2, filtered.Count);
        Assert.Contains(filtered, r => r.Id == 1);
        Assert.Contains(filtered, r => r.Id == 3);
    }
}
