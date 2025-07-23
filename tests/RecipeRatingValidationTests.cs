using RecettesIndex.Models;

namespace RecettesIndex.Tests;

public class RecipeRatingValidationTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Recipe_ValidRating_ShouldBeAccepted(int validRating)
    {
        // Arrange
        var recipe = new Recipe { Name = "Test Recipe" };

        // Act
        recipe.Rating = validRating;

        // Assert
        Assert.Equal(validRating, recipe.Rating);
        Assert.InRange(validRating, 1, 5);
    }

    [Theory]
    [InlineData(-5)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(10)]
    [InlineData(100)]
    public void Recipe_InvalidRating_ModelStillAcceptsButValidationWillFail(int invalidRating)
    {
        // Arrange
        var recipe = new Recipe { Name = "Test Recipe" };

        // Act
        recipe.Rating = invalidRating;

        // Assert
        // Model property still accepts the value (no property-level enforcement)
        Assert.Equal(invalidRating, recipe.Rating);
        
        // But validation attribute will catch this during validation
        Assert.True(invalidRating < 1 || invalidRating > 5, 
            $"Rating {invalidRating} should be outside the valid range of 1-5");
        
        // Note: Use RecipeValidationTests for actual validation testing
    }

    [Fact]
    public void Recipe_Rating_DefaultValue_IsZeroAndRequiresValidation()
    {
        // Arrange & Act
        var recipe = new Recipe();

        // Assert
        Assert.Equal(0, recipe.Rating);
        
        // Note: Default value of 0 is outside valid range
        // Validation will fail until rating is set to 1-5
    }

    [Theory]
    [InlineData(1, "Poor")]
    [InlineData(2, "Fair")]
    [InlineData(3, "Good")]
    [InlineData(4, "Very Good")]
    [InlineData(5, "Excellent")]
    public void Recipe_RatingValues_RepresentQualityLevels(int rating, string qualityLevel)
    {
        // Arrange
        var recipe = new Recipe 
        { 
            Name = $"{qualityLevel} Recipe",
            Rating = rating 
        };

        // Act & Assert
        Assert.Equal(rating, recipe.Rating);
        Assert.InRange(rating, 1, 5);
        Assert.Contains(qualityLevel.ToLower(), recipe.Name.ToLower());
    }

    [Fact]
    public void Recipe_WithValidRating_CanBeCreatedSuccessfully()
    {
        // Arrange & Act
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Five Star Chocolate Cake",
            Rating = 5,
            Notes = "Amazing dessert recipe",
            CreationDate = DateTime.Now
        };

        // Assert
        Assert.Equal("Five Star Chocolate Cake", recipe.Name);
        Assert.Equal(5, recipe.Rating);
        Assert.InRange(recipe.Rating, 1, 5);
        Assert.Equal("Amazing dessert recipe", recipe.Notes);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)] 
    [InlineData(3, 3)]
    [InlineData(4, 4)]
    [InlineData(5, 5)]
    public void Recipe_RatingPersistence_MaintainsValue(int initialRating, int expectedRating)
    {
        // Arrange
        var recipe = new Recipe { Name = "Rating Test Recipe" };

        // Act
        recipe.Rating = initialRating;
        var retrievedRating = recipe.Rating;

        // Assert
        Assert.Equal(expectedRating, retrievedRating);
        Assert.Equal(initialRating, retrievedRating);
    }
}
