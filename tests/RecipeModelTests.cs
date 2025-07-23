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
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Recipe_Rating_AcceptsValidValues(int rating)
    {
        // Arrange
        var recipe = new Recipe();

        // Act
        recipe.Rating = rating;

        // Assert
        Assert.Equal(rating, recipe.Rating);
        Assert.InRange(rating, 1, 5);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(10)]
    [InlineData(100)]
    public void Recipe_Rating_PropertyAcceptsAnyValue_ButValidationWillCatch(int invalidRating)
    {
        // Arrange
        var recipe = new Recipe();

        // Act
        recipe.Rating = invalidRating;

        // Assert
        // The property itself doesn't enforce validation (by design)
        Assert.Equal(invalidRating, recipe.Rating);
        
        // But validation attribute will catch invalid values during validation
        Assert.True(invalidRating < 1 || invalidRating > 5, "This rating should be outside the valid range");
        
        // See RecipeValidationTests for actual validation testing
    }

    [Fact]
    public void Recipe_BookAssociation_CanBeSet()
    {
        // Arrange
        var recipe = new Recipe();
        var book = new Book { Id = 1, Name = "Test Cookbook" };

        // Act
        recipe.BookId = book.Id;
        recipe.Book = book;

        // Assert
        Assert.Equal(book.Id, recipe.BookId);
        Assert.Equal(book, recipe.Book);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Very long recipe name that goes on and on and on to test maximum length handling")]
    public void Recipe_Name_AcceptsVariousStringValues(string name)
    {
        // Arrange
        var recipe = new Recipe();

        // Act
        recipe.Name = name;

        // Assert
        Assert.Equal(name, recipe.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Short note")]
    [InlineData("Very long notes with multiple sentences. This tests how the model handles longer text content.")]
    public void Recipe_Notes_AcceptsVariousValues(string? notes)
    {
        // Arrange
        var recipe = new Recipe();

        // Act
        recipe.Notes = notes;

        // Assert
        Assert.Equal(notes, recipe.Notes);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(5, 250)]
    [InlineData(null, null)]
    public void Recipe_BookPage_AcceptsValidValues(int? bookId, int? page)
    {
        // Arrange
        var recipe = new Recipe();

        // Act
        recipe.BookId = bookId;
        recipe.BookPage = page;

        // Assert
        Assert.Equal(bookId, recipe.BookId);
        Assert.Equal(page, recipe.BookPage);
    }
}