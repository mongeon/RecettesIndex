using RecettesIndex.Models;

namespace RecettesIndex.Tests;

public class RecipeModelTests
{
    [Fact]
    public void Recipe_Creation_SetsPropertiesCorrectly()
    {
        // Arrange
        var expectedName = "Chocolate Chip Cookies";
        var expectedRating = 5;
        var expectedNotes = "Delicious family recipe";
        var expectedBookId = 1;
        var expectedBookPage = 42;
        var expectedCreationDate = DateTime.Now;

        // Act
        var recipe = new Recipe
        {
            Id = 1,
            Name = expectedName,
            Rating = expectedRating,
            Notes = expectedNotes,
            BookId = expectedBookId,
            BookPage = expectedBookPage,
            CreationDate = expectedCreationDate
        };

        // Assert
        Assert.Equal(1, recipe.Id);
        Assert.Equal(expectedName, recipe.Name);
        Assert.Equal(expectedRating, recipe.Rating);
        Assert.Equal(expectedNotes, recipe.Notes);
        Assert.Equal(expectedBookId, recipe.BookId);
        Assert.Equal(expectedBookPage, recipe.BookPage);
        Assert.Equal(expectedCreationDate, recipe.CreationDate);
    }

    [Fact]
    public void Recipe_DefaultValues_AreSetCorrectly()
    {
        // Act
        var recipe = new Recipe();

        // Assert
        Assert.Equal(string.Empty, recipe.Name);
        Assert.Equal(0, recipe.Rating);
        Assert.Null(recipe.Notes);
        Assert.Null(recipe.BookId);
        Assert.Null(recipe.BookPage);
        Assert.Null(recipe.Book);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void Recipe_Rating_AcceptsValidValues(int rating)
    {
        // Arrange
        var recipe = new Recipe();

        // Act
        recipe.Rating = rating;

        // Assert
        Assert.Equal(rating, recipe.Rating);
    }
}