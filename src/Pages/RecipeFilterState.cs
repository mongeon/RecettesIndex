namespace RecettesIndex.Pages;

/// <summary>
/// Encapsulates all filter-related state for the Recipes page,
/// including typed filter values, quick-filter flags, and the
/// derived computed properties that depend on them.
/// </summary>
public class RecipeFilterState
{
    // ── Advanced filter fields ────────────────────────────────────────────────

    public string SearchTerm { get; set; } = string.Empty;
    public string? RatingFilter { get; set; }
    public string? BookFilter { get; set; }
    public string? StoreFilter { get; set; }
    public string? AuthorFilter { get; set; }

    // ── Quick filter flags ────────────────────────────────────────────────────

    public bool ShowAllRecipes { get; set; } = true;
    public int? QuickFilterRating { get; set; }
    public bool QuickFilterUnrated { get; set; }
    public bool ShowRecentRecipes { get; set; }
    public bool ShowFavorites { get; set; }

    // ── Memoized active-filter count ──────────────────────────────────────────

    private int? _cachedActiveFilterCount;

    /// <summary>
    /// Returns the number of active advanced filters (search term, rating, book, store, author).
    /// Result is memoized until <see cref="InvalidateFilterCountCache"/> is called.
    /// </summary>
    public int GetActiveFilterCount()
    {
        if (_cachedActiveFilterCount.HasValue)
        {
            return _cachedActiveFilterCount.Value;
        }

        int count = 0;
        if (!string.IsNullOrWhiteSpace(SearchTerm)) count++;
        if (RatingFilter != null && RatingFilter != "all") count++;
        if (BookFilter != null && BookFilter != "all") count++;
        if (StoreFilter != null && StoreFilter != "all") count++;
        if (AuthorFilter != null && AuthorFilter != "all") count++;

        _cachedActiveFilterCount = count;
        return count;
    }

    /// <summary>Clears the cached active-filter count so it is recomputed on next access.</summary>
    public void InvalidateFilterCountCache() => _cachedActiveFilterCount = null;

    /// <summary>Returns true if any advanced filter is currently active.</summary>
    public bool HasActiveFilters() =>
        !string.IsNullOrWhiteSpace(SearchTerm)
        || (RatingFilter != null && RatingFilter != "all")
        || (BookFilter != null && BookFilter != "all")
        || (StoreFilter != null && StoreFilter != "all")
        || (AuthorFilter != null && AuthorFilter != "all");

    // ── Parsed filter value helpers ───────────────────────────────────────────

    /// <summary>
    /// Returns the effective rating filter integer to pass to the service,
    /// taking quick-filter overrides into account.
    /// </summary>
    public int? GetRatingFilter()
    {
        if (QuickFilterUnrated)
        {
            return null;
        }

        if (QuickFilterRating.HasValue && QuickFilterRating.Value != 4)
        {
            return QuickFilterRating.Value;
        }

        if (RatingFilter != "all" && !string.IsNullOrWhiteSpace(RatingFilter))
        {
            return int.TryParse(RatingFilter, out var r) ? r : null;
        }

        return null;
    }

    /// <summary>Returns the selected book ID, or null when no book filter is active.</summary>
    public int? GetBookIdFilter() =>
        (BookFilter == "all" || string.IsNullOrWhiteSpace(BookFilter))
            ? null
            : (int.TryParse(BookFilter, out var b) ? b : null);

    /// <summary>Returns the selected store ID, or null when no store filter is active.</summary>
    public int? GetStoreIdFilter() =>
        (StoreFilter == "all" || string.IsNullOrWhiteSpace(StoreFilter))
            ? null
            : (int.TryParse(StoreFilter, out var s) ? s : null);

    /// <summary>Returns the selected author ID, or null when no author filter is active.</summary>
    public int? GetAuthorIdFilter() =>
        (AuthorFilter == "all" || string.IsNullOrWhiteSpace(AuthorFilter))
            ? null
            : (int.TryParse(AuthorFilter, out var a) ? a : null);

    // ── Mutation helpers ──────────────────────────────────────────────────────

    /// <summary>Resets all filter fields and quick-filter flags to their defaults.</summary>
    public void Reset()
    {
        SearchTerm = string.Empty;
        RatingFilter = null;
        BookFilter = null;
        StoreFilter = null;
        AuthorFilter = null;
        ShowAllRecipes = true;
        ShowRecentRecipes = false;
        QuickFilterRating = null;
        QuickFilterUnrated = false;
        ShowFavorites = false;
        InvalidateFilterCountCache();
    }

    /// <summary>Clears only the quick-filter flags (rating, unrated, recent), leaving advanced filters intact.</summary>
    public void ResetQuickFilters()
    {
        ShowAllRecipes = false;
        ShowRecentRecipes = false;
        QuickFilterRating = null;
        QuickFilterUnrated = false;
    }
}
