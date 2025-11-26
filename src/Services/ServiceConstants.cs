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

    /// <summary>
    /// Cache key for stores list.
    /// </summary>
    public const string StoresListKey = "stores:list";
}

/// <summary>
/// Constants for pagination configuration.
/// </summary>
public static class PaginationConstants
{
    /// <summary>
    /// Minimum page number (1-based pagination).
    /// </summary>
    public const int MinPage = 1;

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
    /// <summary>
    /// Sort by recipe name.
    /// </summary>
    public const string Name = "name";

    /// <summary>
    /// Sort by recipe rating.
    /// </summary>
    public const string Rating = "rating";

    /// <summary>
    /// Sort by recipe creation date.
    /// </summary>
    public const string CreatedAt = "created_at";
}
