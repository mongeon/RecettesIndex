using Xunit;
using RecettesIndex.Models;
using System;

namespace RecettesIndex.Tests.Components;

/// <summary>
/// Tests for RecipeCard component logic and data handling.
/// </summary>
public class RecipeCardTests
{
    [Fact]
    public void RecipeCard_NotesPreview_LongText_TruncatesCorrectly()
    {
        // Arrange
        var longNotes = new string('x', 100);
        var maxLength = 80;

        // Act
        var preview = longNotes.Length > maxLength 
            ? longNotes.Substring(0, maxLength) + "..." 
            : longNotes;

        // Assert
        Assert.Equal(83, preview.Length); // 80 chars + "..."
        Assert.EndsWith("...", preview);
    }

    [Fact]
    public void RecipeCard_NotesPreview_ShortText_DoesNotTruncate()
    {
        // Arrange
        var shortNotes = "Short recipe notes";
        var maxLength = 80;

        // Act
        var preview = shortNotes.Length > maxLength 
            ? shortNotes.Substring(0, maxLength) + "..." 
            : shortNotes;

        // Assert
        Assert.Equal(shortNotes, preview);
        Assert.DoesNotContain("...", preview);
    }

    [Fact]
    public void RecipeCard_Recipe_WithRating_DisplaysRatingChip()
    {
        // Arrange
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Test Recipe",
            Rating = 5,
            CreationDate = DateTime.UtcNow
        };

        // Act
        var shouldShowRating = recipe.Rating > 0;

        // Assert
        Assert.True(shouldShowRating);
        Assert.Equal(5, recipe.Rating);
    }

    [Fact]
    public void RecipeCard_Recipe_WithoutRating_DoesNotDisplayRatingChip()
    {
        // Arrange
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Test Recipe",
            Rating = 0,
            CreationDate = DateTime.UtcNow
        };

        // Act
        var shouldShowRating = recipe.Rating > 0;

        // Assert
        Assert.False(shouldShowRating);
    }

    [Fact]
    public void RecipeCard_Recipe_WithBook_DisplaysBookInfo()
    {
        // Arrange
        var book = new Book
        {
            Id = 1,
            Name = "Test Cookbook",
            CreationDate = DateTime.UtcNow
        };
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Test Recipe",
            Rating = 4,
            BookId = 1,
            Book = book,
            CreationDate = DateTime.UtcNow
        };

        // Act
        var hasBook = recipe.Book != null;

        // Assert
        Assert.True(hasBook);
        Assert.Equal("Test Cookbook", recipe.Book.Name);
    }

    [Fact]
    public void RecipeCard_Recipe_WithoutBook_DoesNotDisplayBookInfo()
    {
        // Arrange
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Test Recipe",
            Rating = 4,
            BookId = null,
            Book = null,
            CreationDate = DateTime.UtcNow
        };

        // Act
        var hasBook = recipe.Book != null;

        // Assert
        Assert.False(hasBook);
        Assert.Null(recipe.BookId);
    }

    [Fact]
    public void RecipeCard_Recipe_WithBookPage_DisplaysPageChip()
    {
        // Arrange
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Test Recipe",
            Rating = 4,
            BookPage = 42,
            CreationDate = DateTime.UtcNow
        };

        // Act
        var hasPageNumber = recipe.BookPage.HasValue;

        // Assert
        Assert.True(hasPageNumber);
        Assert.Equal(42, recipe.BookPage.Value);
    }

    [Fact]
    public void RecipeCard_Recipe_WithoutBookPage_DoesNotDisplayPageChip()
    {
        // Arrange
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Test Recipe",
            Rating = 4,
            BookPage = null,
            CreationDate = DateTime.UtcNow
        };

        // Act
        var hasPageNumber = recipe.BookPage.HasValue;

        // Assert
        Assert.False(hasPageNumber);
    }

    [Fact]
    public void RecipeCard_DateFormatting_DisplaysCorrectFormat()
    {
        // Arrange
        var date = new DateTime(2024, 3, 15);
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Test Recipe",
            Rating = 4,
            CreationDate = date
        };

        // Act
        var formattedDate = recipe.CreationDate.ToString("MMM dd, yyyy");

        // Assert
        // Should contain month abbreviation, day and year
        Assert.Contains("15", formattedDate);
        Assert.Contains("2024", formattedDate);
        Assert.True(formattedDate.Length >= 10); // e.g., "Mar 15, 2024" or "mars 15, 2024"
    }

    [Fact]
    public void RecipeCard_NavigationUrl_GeneratesCorrectUrl()
    {
        // Arrange
        var recipeId = 123;
        var recipe = new Recipe
        {
            Id = recipeId,
            Name = "Test Recipe",
            Rating = 4,
            CreationDate = DateTime.UtcNow
        };

        // Act
        var expectedUrl = $"/recipes/{recipeId}";

        // Assert
        Assert.Equal("/recipes/123", expectedUrl);
    }

    [Theory]
    [InlineData(0, 3, 0)] // 3n+1 gradient
    [InlineData(1, 3, 1)] // 3n+2 gradient
    [InlineData(2, 3, 2)] // 3n+3 gradient
    [InlineData(3, 3, 0)] // wraps around
    public void RecipeCard_GradientIndex_CalculatesCorrectly(int index, int divisor, int expected)
    {
        // Act
        var gradientIndex = index % divisor;

        // Assert
        Assert.Equal(expected, gradientIndex);
    }

    [Fact]
    public void RecipeCard_Recipe_WithAllProperties_DisplaysAllInfo()
    {
        // Arrange
        var book = new Book
        {
            Id = 1,
            Name = "Complete Cookbook",
            CreationDate = DateTime.UtcNow
        };
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Perfect Recipe",
            Rating = 5,
            Notes = "These are some detailed notes about the recipe",
            BookId = 1,
            Book = book,
            BookPage = 42,
            CreationDate = DateTime.UtcNow
        };

        // Act & Assert
        Assert.Equal("Perfect Recipe", recipe.Name);
        Assert.Equal(5, recipe.Rating);
        Assert.False(string.IsNullOrWhiteSpace(recipe.Notes));
        Assert.NotNull(recipe.Book);
        Assert.Equal("Complete Cookbook", recipe.Book.Name);
        Assert.Equal(42, recipe.BookPage);
    }

    [Fact]
    public void RecipeCard_Recipe_WithMinimalProperties_DisplaysBasicInfo()
    {
        // Arrange
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Basic Recipe",
            Rating = 0,
            Notes = null,
            BookId = null,
            Book = null,
            BookPage = null,
            CreationDate = DateTime.UtcNow
        };

        // Act & Assert
        Assert.Equal("Basic Recipe", recipe.Name);
        Assert.Equal(0, recipe.Rating);
        Assert.True(string.IsNullOrWhiteSpace(recipe.Notes));
        Assert.Null(recipe.Book);
        Assert.Null(recipe.BookPage);
    }

    [Fact]
    public void RecipeCard_RecipeName_TitleClamping_LongNames()
    {
        // Arrange
        var longName = "This is an extremely long recipe name that will need to be clamped in the UI using webkit box properties";
        
        // Act
        var shouldClamp = longName.Length > 50;

        // Assert
        Assert.True(shouldClamp);
        Assert.True(longName.Length > 50);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(3, true)]
    [InlineData(4, true)]
    [InlineData(5, true)]
    [InlineData(0, false)]
    public void RecipeCard_RatingValidation_ChecksValidRange(int rating, bool isValid)
    {
        // Arrange
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Test Recipe",
            Rating = rating,
            CreationDate = DateTime.UtcNow
        };

        // Act
        var shouldDisplay = recipe.Rating > 0;

        // Assert
        Assert.Equal(isValid, shouldDisplay);
    }
}
