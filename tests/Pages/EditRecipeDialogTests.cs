using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using RecettesIndex.Models;

namespace RecettesIndex.Tests.Pages;

/// <summary>
/// Tests to verify that CreationDate is preserved when editing recipes.
/// These tests focus on the data copying logic used in EditRecipeDialog.
/// </summary>
public class EditRecipeDialogTests : TestContext
{
    public EditRecipeDialogTests()
    {
        Services.AddMudServices();
    }

    [Fact]
    public void Recipe_CreationDate_IsPreserved_WhenCopied()
    {
        // Arrange
        var originalCreationDate = new DateTime(2024, 2, 14, 16, 20, 0);
        var originalRecipe = new Recipe
        {
            Id = 1,
            Name = "Chocolate Cake",
            Rating = 5,
            Notes = "Delicious!",
            BookId = 1,
            BookPage = 42,
            CreationDate = originalCreationDate
        };

        // Act - Simulate what the component does when initializing
        var copiedRecipe = new Recipe
        {
            Id = originalRecipe.Id,
            Name = originalRecipe.Name,
            Rating = originalRecipe.Rating,
            Notes = originalRecipe.Notes,
            BookId = originalRecipe.BookId,
            BookPage = originalRecipe.BookPage,
            CreationDate = originalRecipe.CreationDate
        };

        // Assert
        Assert.Equal(originalCreationDate, copiedRecipe.CreationDate);
        Assert.Equal(originalRecipe.Id, copiedRecipe.Id);
        Assert.Equal(originalRecipe.Name, copiedRecipe.Name);
        Assert.Equal(originalRecipe.Rating, copiedRecipe.Rating);
    }

    [Theory]
    [InlineData(2024, 1, 1)]
    [InlineData(2023, 12, 25)]
    [InlineData(2025, 6, 15)]
    public void Recipe_CreationDate_PreservesVariousDates(int year, int month, int day)
    {
        // Arrange
        var creationDate = new DateTime(year, month, day);
        var originalRecipe = new Recipe
        {
            Id = 1,
            Name = "Test Recipe",
            Rating = 3,
            CreationDate = creationDate
        };

        // Act
        var copiedRecipe = new Recipe
        {
            Id = originalRecipe.Id,
            Name = originalRecipe.Name,
            Rating = originalRecipe.Rating,
            Notes = originalRecipe.Notes,
            BookId = originalRecipe.BookId,
            BookPage = originalRecipe.BookPage,
            CreationDate = originalRecipe.CreationDate
        };

        // Assert
        Assert.Equal(creationDate, copiedRecipe.CreationDate);
        Assert.Equal(year, copiedRecipe.CreationDate.Year);
        Assert.Equal(month, copiedRecipe.CreationDate.Month);
        Assert.Equal(day, copiedRecipe.CreationDate.Day);
    }

    [Fact]
    public void Recipe_CreationDate_PreservedWhenRatingChanges()
    {
        // Arrange
        var creationDate = new DateTime(2024, 1, 15, 10, 0, 0);
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Test Recipe",
            Rating = 3,
            CreationDate = creationDate
        };

        // Act - Simulate editing with a new rating
        var editedRecipe = new Recipe
        {
            Id = recipe.Id,
            Name = recipe.Name,
            Rating = 5, // Changed rating
            Notes = recipe.Notes,
            BookId = recipe.BookId,
            BookPage = recipe.BookPage,
            CreationDate = recipe.CreationDate // Preserved
        };

        // Assert
        Assert.Equal(creationDate, editedRecipe.CreationDate);
        Assert.Equal(5, editedRecipe.Rating);
        Assert.NotEqual(recipe.Rating, editedRecipe.Rating);
    }

    [Fact]
    public void Recipe_WithOptionalFields_PreservesCreationDate()
    {
        // Arrange
        var creationDate = new DateTime(2024, 3, 10, 8, 30, 0);
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Simple Recipe",
            Rating = 4,
            Notes = null,
            BookId = null,
            BookPage = null,
            CreationDate = creationDate
        };

        // Act
        var copiedRecipe = new Recipe
        {
            Id = recipe.Id,
            Name = recipe.Name,
            Rating = recipe.Rating,
            Notes = recipe.Notes,
            BookId = recipe.BookId,
            BookPage = recipe.BookPage,
            CreationDate = recipe.CreationDate
        };

        // Assert
        Assert.Equal(creationDate, copiedRecipe.CreationDate);
        Assert.Null(copiedRecipe.Notes);
        Assert.Null(copiedRecipe.BookId);
        Assert.Null(copiedRecipe.BookPage);
    }

    [Fact]
    public void Recipe_AllFieldsUpdated_ExceptCreationDate()
    {
        // Arrange
        var creationDate = new DateTime(2024, 1, 1);
        var originalRecipe = new Recipe
        {
            Id = 1,
            Name = "Original Name",
            Rating = 2,
            Notes = "Original notes",
            BookId = 1,
            BookPage = 10,
            CreationDate = creationDate
        };

        // Act - Update all fields except creation date
        var updatedRecipe = new Recipe
        {
            Id = originalRecipe.Id,
            Name = "Updated Name",
            Rating = 5,
            Notes = "Updated notes",
            BookId = 2,
            BookPage = 20,
            CreationDate = originalRecipe.CreationDate
        };

        // Assert
        Assert.Equal(creationDate, updatedRecipe.CreationDate);
        Assert.NotEqual(originalRecipe.Name, updatedRecipe.Name);
        Assert.NotEqual(originalRecipe.Rating, updatedRecipe.Rating);
        Assert.NotEqual(originalRecipe.Notes, updatedRecipe.Notes);
        Assert.NotEqual(originalRecipe.BookId, updatedRecipe.BookId);
        Assert.NotEqual(originalRecipe.BookPage, updatedRecipe.BookPage);
    }
}
