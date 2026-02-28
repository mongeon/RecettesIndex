using RecettesIndex.Services.Abstractions;

namespace RecettesIndex.Services;

/// <summary>
/// Extension methods for ILocalStorageService to manage recent recipes.
/// </summary>
public static class RecentRecipesExtensions
{
    private const string RecentRecipesKey = "recentRecipes";
    private const int MaxRecentRecipes = 5;

    /// <summary>
    /// Adds a recipe to the recent recipes list.
    /// </summary>
    public static async Task AddRecentRecipeAsync(this ILocalStorageService localStorage, int recipeId, string recipeName)
    {
        var recentRecipes = await localStorage.GetRecentRecipesAsync();

        // Remove if already exists (to avoid duplicates)
        recentRecipes.RemoveAll(r => r.Id == recipeId);

        // Add to the beginning of the list
        recentRecipes.Insert(0, new RecentRecipe
        {
            Id = recipeId,
            Name = recipeName,
            ViewedAt = DateTime.UtcNow
        });

        // Keep only the most recent N recipes
        if (recentRecipes.Count > MaxRecentRecipes)
        {
            recentRecipes = recentRecipes.Take(MaxRecentRecipes).ToList();
        }

        await localStorage.SetItemAsync(RecentRecipesKey, recentRecipes);
    }

    /// <summary>
    /// Gets the list of recent recipes.
    /// </summary>
    public static async Task<List<RecentRecipe>> GetRecentRecipesAsync(this ILocalStorageService localStorage)
    {
        var recentRecipes = await localStorage.GetItemAsync<List<RecentRecipe>>(RecentRecipesKey);
        return recentRecipes ?? new List<RecentRecipe>();
    }

    /// <summary>
    /// Clears all recent recipes.
    /// </summary>
    public static async Task ClearRecentRecipesAsync(this ILocalStorageService localStorage)
    {
        await localStorage.RemoveItemAsync(RecentRecipesKey);
    }
}
