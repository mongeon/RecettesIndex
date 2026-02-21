namespace RecettesIndex.Models;

/// <summary>
/// Aggregated statistics data for the Dashboard page.
/// </summary>
public class DashboardStatistics
{
    public int TotalRecipes { get; set; }
    public int TotalBooks { get; set; }
    public int TotalAuthors { get; set; }
    public int TotalStores { get; set; }
    public double AverageRating { get; set; }
    public Dictionary<int, int> RatingDistribution { get; set; } = new();
    public int UnratedCount { get; set; }
    public List<Recipe> TopRatedRecipes { get; set; } = new();
    public List<BookStatistic> MostUsedBooks { get; set; } = new();
    public List<StoreStatistic> MostUsedStores { get; set; } = new();
    public List<AuthorStatistic> MostUsedAuthors { get; set; } = new();
    public Recipe? MostRecentRecipe { get; set; }
    public Book? MostRecentBook { get; set; }
    public int RecipesAddedLast7Days { get; set; }
    public int RecipesAddedLast30Days { get; set; }
}

/// <summary>
/// Book usage statistics for the Dashboard.
/// </summary>
public class BookStatistic
{
    public int BookId { get; set; }
    public string BookName { get; set; } = string.Empty;
    public int RecipeCount { get; set; }
}

/// <summary>
/// Store usage statistics for the Dashboard.
/// </summary>
public class StoreStatistic
{
    public int StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public int RecipeCount { get; set; }
}

/// <summary>
/// Author usage statistics for the Dashboard.
/// </summary>
public class AuthorStatistic
{
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public int RecipeCount { get; set; }
}
