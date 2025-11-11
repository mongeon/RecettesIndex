namespace RecettesIndex.Services;

/// <summary>
/// Constants for caching configuration.
/// </summary>
public static class CacheConstants
{
    /// <summary>
    /// Default time-to-live for cache entries.
    /// </summary>
    public static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(3);
    
    /// <summary>
    /// Cache key for books list.
    /// </summary>
    public const string BooksListKey = "books:list";
    
    /// <summary>
    /// Cache key for authors list.
    /// </summary>
    public const string AuthorsListKey = "authors:list";
}

/// <summary>
/// Constants for pagination configuration.
/// </summary>
public static class PaginationConstants
{
    /// <summary>
    /// Minimum allowed page size.
    /// </summary>
    public const int MinPageSize = 1;
    
    /// <summary>
    /// Maximum allowed page size to prevent excessive data loading.
    /// </summary>
    public const int MaxPageSize = 100;
    
    /// <summary>
    /// Default page size for data grids and lists.
    /// </summary>
    public const int DefaultPageSize = 20;
}

/// <summary>
/// Constants for recipe sorting.
/// </summary>
public static class RecipeSortConstants
{
    public const string Name = "name";
    public const string Rating = "rating";
    public const string CreatedAt = "created_at";
}
