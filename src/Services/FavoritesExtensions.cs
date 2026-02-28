using RecettesIndex.Services.Abstractions;

namespace RecettesIndex.Services;

/// <summary>
/// Extension methods for ILocalStorageService to manage favorite recipes.
/// </summary>
public static class FavoritesExtensions
{
    private const string FavoritesKey = "favoriteRecipes";

    /// <summary>
    /// Gets the list of favorite recipe IDs.
    /// </summary>
    public static async Task<List<int>> GetFavoritesAsync(this ILocalStorageService localStorage)
    {
        var favorites = await localStorage.GetItemAsync<List<int>>(FavoritesKey);
        return favorites ?? new List<int>();
    }

    /// <summary>
    /// Checks if a recipe is marked as favorite.
    /// </summary>
    public static async Task<bool> IsFavoriteAsync(this ILocalStorageService localStorage, int recipeId)
    {
        var favorites = await localStorage.GetFavoritesAsync();
        return favorites.Contains(recipeId);
    }

    /// <summary>
    /// Toggles favorite status for a recipe.
    /// </summary>
    public static async Task ToggleFavoriteAsync(this ILocalStorageService localStorage, int recipeId)
    {
        var favorites = await localStorage.GetFavoritesAsync();

        if (favorites.Contains(recipeId))
        {
            favorites.Remove(recipeId);
        }
        else
        {
            favorites.Add(recipeId);
        }

        await localStorage.SetItemAsync(FavoritesKey, favorites);
    }

    /// <summary>
    /// Clears all favorite recipes.
    /// </summary>
    public static async Task ClearFavoritesAsync(this ILocalStorageService localStorage)
    {
        await localStorage.RemoveItemAsync(FavoritesKey);
    }

    /// <summary>
    /// Gets the count of favorite recipes.
    /// </summary>
    public static async Task<int> GetFavoritesCountAsync(this ILocalStorageService localStorage)
    {
        var favorites = await localStorage.GetFavoritesAsync();
        return favorites.Count;
    }
}
