using RecettesIndex.Models;
using System.ComponentModel.DataAnnotations;

namespace RecettesIndex.Tests;

public class RecipeValidationTests
{
    private List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Recipe_ValidRating_PassesValidation(int validRating)
    {
        // Arrange
        var recipe = new Recipe 
        { 
            Name = "Test Recipe",
            Rating = validRating 
        };

        // Act
        var validationResults = ValidateModel(recipe);

        // Assert
        Assert.Empty(validationResults);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(10)]
    [InlineData(100)]
    public void Recipe_InvalidRating_FailsValidation(int invalidRating)
    {
        // Arrange
        var recipe = new Recipe 
        { 
            Name = "Test Recipe",
            Rating = invalidRating 
        };

        // Act
        var validationResults = ValidateModel(recipe);

        // Assert
        Assert.Single(validationResults);
        Assert.Equal("Rating must be between 1 and 5", validationResults[0].ErrorMessage);
        Assert.Contains("Rating", validationResults[0].MemberNames);
    }

    [Fact]
    public void Recipe_DefaultRating_FailsValidation()
    {
        // Arrange
        var recipe = new Recipe { Name = "Test Recipe" };
        // Default rating is 0, which is invalid

        // Act
        var validationResults = ValidateModel(recipe);

        // Assert
        Assert.Single(validationResults);
        Assert.Equal("Rating must be between 1 and 5", validationResults[0].ErrorMessage);
    }

    [Fact]
    public void Recipe_CompleteValidRecipe_PassesValidation()
    {
        // Arrange
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Chocolate Chip Cookies",
            Rating = 5,
            Notes = "Delicious family recipe",
            BookId = 1,
            BookPage = 42,
            CreationDate = DateTime.Now
        };

        // Act
        var validationResults = ValidateModel(recipe);

        // Assert
        Assert.Empty(validationResults);
    }

    [Theory]
    [InlineData(1, "Poor")]
    [InlineData(2, "Fair")]
    [InlineData(3, "Good")]
    [InlineData(4, "Very Good")]
    [InlineData(5, "Excellent")]
    public void Recipe_ValidRatingWithDescription_PassesValidation(int rating, string description)
    {
        // Arrange
        var recipe = new Recipe
        {
            Name = $"{description} Recipe",
            Rating = rating,
            Notes = $"This is a {description.ToLower()} recipe"
        };

        // Act
        var validationResults = ValidateModel(recipe);

        // Assert
        Assert.Empty(validationResults);
        Assert.InRange(recipe.Rating, 1, 5);
    }

    [Fact]
    public void Recipe_EmptyName_FailsValidation()
    {
        // Arrange
        var recipe = new Recipe
        {
            Name = "", // Empty name - should now fail validation
            Rating = 3 // Valid rating
        };

        // Act
        var validationResults = ValidateModel(recipe);

        // Assert
        // Name field is now marked as Required, so empty name should fail validation
        Assert.Single(validationResults);
        var validationResult = validationResults.Single();
        Assert.Equal("Name", validationResult.MemberNames.Single());
        Assert.Equal("The Name field is required.", validationResult.ErrorMessage);
    }

    [Fact]
    public void Recipe_NullName_FailsValidation()
    {
        // Arrange
        var recipe = new Recipe
        {
            Name = null!, // Null name - should fail validation
            Rating = 3 // Valid rating
        };

        // Act
        var validationResults = ValidateModel(recipe);

        // Assert
        // Name field is now marked as Required, so null name should fail validation
        Assert.Single(validationResults);
        var validationResult = validationResults.Single();
        Assert.Equal("Name", validationResult.MemberNames.Single());
        Assert.Equal("The Name field is required.", validationResult.ErrorMessage);
    }

    [Fact]
    public void Recipe_ValidationError_ContainsCorrectMemberName()
    {
        // Arrange
        var recipe = new Recipe 
        { 
            Name = "Test Recipe",
            Rating = 0 // Invalid
        };

        // Act
        var validationResults = ValidateModel(recipe);

        // Assert
        Assert.Single(validationResults);
        var validationResult = validationResults[0];
        Assert.Equal("Rating", validationResult.MemberNames.Single());
        Assert.Equal("Rating must be between 1 and 5", validationResult.ErrorMessage);
    }
}
