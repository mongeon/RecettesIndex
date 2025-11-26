using RecettesIndex.Models;

namespace RecettesIndex.Tests.Integration;

public class RecipeStoreIntegrationTests
{
    [Fact]
    public void Recipe_CanBeLinkedToStore()
    {
        // Arrange
        var store = new Store { Id = 1, Name = "Le Gourmet Express" };
        var recipe = new Recipe { Id = 1, Name = "Poulet Rôti", Rating = 4 };

        // Act
        recipe.StoreId = store.Id;
        recipe.Store = store;

        // Assert
        Assert.Equal(store.Id, recipe.StoreId);
        Assert.Equal(store, recipe.Store);
        Assert.True(recipe.IsFromStore);
        Assert.False(recipe.IsFromBook);
    }

    [Fact]
    public void Recipe_IsFromStore_ReturnsTrueWhenStoreIdSet()
    {
        // Arrange
        var recipe = new Recipe { Id = 1, Name = "Test Recipe", Rating = 3, StoreId = 1 };

        // Act & Assert
        Assert.True(recipe.IsFromStore);
        Assert.False(recipe.IsFromBook);
    }

    [Fact]
    public void Recipe_IsFromBook_ReturnsTrueWhenBookIdSet()
    {
        // Arrange
        var recipe = new Recipe { Id = 1, Name = "Test Recipe", Rating = 3, BookId = 1 };

        // Act & Assert
        Assert.True(recipe.IsFromBook);
        Assert.False(recipe.IsFromStore);
    }

    [Fact]
    public void Recipe_WithNoSource_ReturnsFalseForBoth()
    {
        // Arrange
        var recipe = new Recipe { Id = 1, Name = "Test Recipe", Rating = 3 };

        // Act & Assert
        Assert.False(recipe.IsFromBook);
        Assert.False(recipe.IsFromStore);
    }

    [Fact]
    public void Recipe_SourceName_ReturnsStoreNameWhenLinkedToStore()
    {
        // Arrange
        var store = new Store { Id = 1, Name = "Le Gourmet Express" };
        var recipe = new Recipe 
        { 
            Id = 1, 
            Name = "Poulet Rôti",
            Rating = 4,
            StoreId = store.Id,
            Store = store
        };

        // Act & Assert
        Assert.Equal("Le Gourmet Express", recipe.SourceName);
    }

    [Fact]
    public void Recipe_SourceName_ReturnsBookNameWhenLinkedToBook()
    {
        // Arrange
        var book = new Book { Id = 1, Name = "The Joy of Cooking" };
        var recipe = new Recipe 
        { 
            Id = 1, 
            Name = "Chocolate Cake",
            Rating = 5,
            BookId = book.Id,
            Book = book
        };

        // Act & Assert
        Assert.Equal("The Joy of Cooking", recipe.SourceName);
    }

    [Fact]
    public void Recipe_SourceName_ReturnsNullWhenNoSource()
    {
        // Arrange
        var recipe = new Recipe { Id = 1, Name = "Test Recipe", Rating = 3 };

        // Act & Assert
        Assert.Null(recipe.SourceName);
    }

    [Fact]
    public void Recipe_CanHaveRecipeType()
    {
        // Arrange
        var recipe = new Recipe 
        { 
            Id = 1, 
            Name = "Store-Bought Lasagna",
            Rating = 4,
            StoreId = 1,
            RecipeType = "store"
        };

        // Act & Assert
        Assert.Equal("store", recipe.RecipeType);
        Assert.True(recipe.IsFromStore);
    }

    [Theory]
    [InlineData("homemade")]
    [InlineData("store")]
    [InlineData("restaurant")]
    public void Recipe_RecipeType_AcceptsValidValues(string recipeType)
    {
        // Arrange
        var recipe = new Recipe 
        { 
            Id = 1, 
            Name = "Test Recipe",
            Rating = 3,
            RecipeType = recipeType
        };

        // Act & Assert
        Assert.Equal(recipeType, recipe.RecipeType);
    }

    [Fact]
    public void Store_CanHaveMultipleRecipes()
    {
        // Arrange
        var store = new Store { Id = 1, Name = "Le Gourmet Express" };
        var recipe1 = new Recipe { Id = 1, Name = "Poulet Rôti", Rating = 4, StoreId = store.Id, Store = store };
        var recipe2 = new Recipe { Id = 2, Name = "Lasagne", Rating = 5, StoreId = store.Id, Store = store };
        var recipe3 = new Recipe { Id = 3, Name = "Salade César", Rating = 3, StoreId = store.Id, Store = store };

        // Act & Assert
        Assert.Equal(store.Id, recipe1.StoreId);
        Assert.Equal(store.Id, recipe2.StoreId);
        Assert.Equal(store.Id, recipe3.StoreId);
        Assert.All(new[] { recipe1, recipe2, recipe3 }, r => Assert.True(r.IsFromStore));
    }

    [Fact]
    public void Recipe_StoreAndBookIdCanBothBeNull()
    {
        // Arrange & Act
        var recipe = new Recipe 
        { 
            Id = 1, 
            Name = "Family Recipe",
            Rating = 5,
            BookId = null,
            StoreId = null
        };

        // Assert
        Assert.Null(recipe.BookId);
        Assert.Null(recipe.StoreId);
        Assert.False(recipe.IsFromBook);
        Assert.False(recipe.IsFromStore);
        Assert.Null(recipe.SourceName);
    }
}
