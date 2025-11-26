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

    [Fact]
    public void Recipe_IsFromUrl_ReturnsTrueWhenUrlSet()
    {
        // Arrange
        var recipe = new Recipe { Id = 1, Name = "Test Recipe", Rating = 3, Url = "https://example.com/recipe" };

        // Act & Assert
        Assert.True(recipe.IsFromUrl);
        Assert.False(recipe.IsFromBook);
        Assert.False(recipe.IsFromStore);
    }

    [Fact]
    public void Recipe_IsFromUrl_ReturnsFalseWhenUrlIsNullOrEmpty()
    {
        // Arrange
        var recipe1 = new Recipe { Id = 1, Name = "Test Recipe", Rating = 3, Url = null };
        var recipe2 = new Recipe { Id = 2, Name = "Test Recipe", Rating = 3, Url = string.Empty };
        var recipe3 = new Recipe { Id = 3, Name = "Test Recipe", Rating = 3, Url = "   " };

        // Act & Assert
        Assert.False(recipe1.IsFromUrl);
        Assert.False(recipe2.IsFromUrl);
        Assert.False(recipe3.IsFromUrl);
    }

    [Fact]
    public void Recipe_SourceName_ReturnsWebsiteWhenFromUrl()
    {
        // Arrange
        var recipe = new Recipe 
        { 
            Id = 1, 
            Name = "Online Recipe",
            Rating = 4,
            Url = "https://example.com/recipe"
        };

        // Act & Assert
        Assert.Equal("Website", recipe.SourceName);
    }

    [Fact]
    public void Recipe_SourceName_PrioritizesBookOverUrl()
    {
        // Arrange
        var book = new Book { Id = 1, Name = "The Joy of Cooking" };
        var recipe = new Recipe 
        { 
            Id = 1, 
            Name = "Chocolate Cake",
            Rating = 5,
            BookId = book.Id,
            Book = book,
            Url = "https://example.com/recipe" // This should be ignored
        };

        // Act & Assert
        Assert.Equal("The Joy of Cooking", recipe.SourceName);
        Assert.True(recipe.IsFromBook);
        Assert.True(recipe.IsFromUrl); // Both can be true, but SourceName prioritizes book
    }
}
