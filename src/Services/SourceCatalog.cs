using RecettesIndex.Models;

namespace RecettesIndex.Services;

/// <summary>One place a recipe can come from: a book, a shop, or a website.</summary>
/// <param name="Kind">Which of the three, using the same vocabulary as the detail banner.</param>
/// <param name="Key">Stable identity across the three kinds — what selection is keyed by.</param>
/// <param name="Id">Book or shop identifier; null for a website, which has no row of its own.</param>
/// <param name="Subtitle">The author of a book, the address of a shop, « site web » otherwise.</param>
/// <param name="LastUsed">When a recipe was last added from here; null when none ever was.</param>
public sealed record SourceRow(
    RecipeSourceKind Kind,
    string Key,
    int? Id,
    string Name,
    string Subtitle,
    int RecipeCount,
    double AverageRating,
    DateTime? LastUsed,
    IReadOnlyList<int> AuthorIds);

/// <summary>The counters carried by the filter pills.</summary>
public sealed record SourceCounts(int All, int Books, int Stores, int Sites, int NeverUsed);

/// <summary>
/// Builds the single list that replaces the Books, Stores and Authors pages.
/// </summary>
/// <remarks>
/// Those were three near-identical tables, and an author has no content of its own — it is
/// an attribute of a book. So authors become a filter here rather than a fourth page.
///
/// Pure, like the rest of the redesign's arithmetic: it takes the recipes, the books and
/// the shops, and returns rows. Websites have no table of their own, so they are derived
/// from the recipes that point at them, one row per domain.
/// </remarks>
public static class SourceCatalog
{
    public static List<SourceRow> Build(
        IReadOnlyList<Recipe> recipes,
        IReadOnlyList<Book> books,
        IReadOnlyList<Store> stores)
    {
        ArgumentNullException.ThrowIfNull(recipes);
        ArgumentNullException.ThrowIfNull(books);
        ArgumentNullException.ThrowIfNull(stores);

        var rows = new List<SourceRow>();

        // Un seul parcours des recettes pour tous les livres, puis pour tous les
        // commerces : la version naïve rescanne la collection entière une fois par
        // source, soit un coût produit qui se paie côté client.
        var byBook = recipes.Where(r => r.BookId.HasValue).ToLookup(r => r.BookId!.Value);
        var byStore = recipes.Where(r => r.StoreId.HasValue).ToLookup(r => r.StoreId!.Value);

        foreach (var book in books)
        {
            var own = byBook[book.Id].ToList();
            rows.Add(new SourceRow(
                RecipeSourceKind.Book,
                $"book:{book.Id}",
                book.Id,
                book.Name,
                book.Authors is { Count: > 0 } authors
                    ? string.Join(" et ", authors.Select(a => a.FullName))
                    : string.Empty,
                own.Count,
                Average(own),
                own.Count == 0 ? null : own.Max(r => r.CreationDate),
                book.Authors?.Select(a => a.Id).ToList() ?? []));
        }

        foreach (var store in stores)
        {
            var own = byStore[store.Id].ToList();
            rows.Add(new SourceRow(
                RecipeSourceKind.Store,
                $"store:{store.Id}",
                store.Id,
                store.Name,
                store.Address ?? string.Empty,
                own.Count,
                Average(own),
                own.Count == 0 ? null : own.Max(r => r.CreationDate),
                []));
        }

        // Un site n'a pas de table : cinq recettes du même domaine sont une source, pas
        // cinq. Une source de ce type ne peut donc jamais être « jamais ouverte » — elle
        // n'existe que parce qu'une recette en vient.
        var byDomain = recipes
            .Where(r => RecipeSource.Classify(r) == RecipeSourceKind.Web)
            .GroupBy(r => RecipeSource.Domain(r.Url))
            .Where(g => g.Key.Length > 0);

        foreach (var group in byDomain)
        {
            var own = group.ToList();
            rows.Add(new SourceRow(
                RecipeSourceKind.Web,
                $"web:{group.Key}",
                null,
                group.Key,
                "site web",
                own.Count,
                Average(own),
                own.Max(r => r.CreationDate),
                []));
        }

        return rows;
    }

    public static SourceCounts Count(IReadOnlyList<SourceRow> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        return new SourceCounts(
            rows.Count,
            rows.Count(r => r.Kind == RecipeSourceKind.Book),
            rows.Count(r => r.Kind == RecipeSourceKind.Store),
            rows.Count(r => r.Kind == RecipeSourceKind.Web),
            rows.Count(r => r.RecipeCount == 0));
    }

    /// <summary>
    /// Narrows the list by the filter bar's controls. Every argument left null is simply
    /// not applied, so the same call serves the unfiltered view.
    /// </summary>
    public static List<SourceRow> Filter(
        IReadOnlyList<SourceRow> rows,
        string? text = null,
        RecipeSourceKind? kind = null,
        int? authorId = null,
        bool neverUsedOnly = false)
    {
        ArgumentNullException.ThrowIfNull(rows);

        IEnumerable<SourceRow> query = rows;

        if (kind.HasValue)
        {
            query = query.Where(r => r.Kind == kind.Value);
        }

        if (authorId.HasValue)
        {
            query = query.Where(r => r.AuthorIds.Contains(authorId.Value));
        }

        if (neverUsedOnly)
        {
            query = query.Where(r => r.RecipeCount == 0);
        }

        // Même filtrage tolérant que la palette ⌘K : « ricardo » trouve « Ricardo », et
        // le nom de l'auteur compte autant que celui du livre pour retrouver un livre.
        var normalized = SearchText.Normalize(text);
        if (normalized.Length > 0)
        {
            var terms = SearchText.Terms(normalized);
            query = query.Where(r =>
            {
                var haystack = SearchText.Normalize($"{r.Name} {r.Subtitle}");
                return terms.All(t => SearchText.ContainsTerm(haystack, t));
            });
        }

        return query.ToList();
    }

    /// <summary>
    /// Sorts by one of the labels the sort menu offers, defaulting to the recipe count —
    /// the question « what do I actually use » is the one this page exists to answer.
    /// </summary>
    public static List<SourceRow> Sort(IReadOnlyList<SourceRow> rows, string? sort)
    {
        ArgumentNullException.ThrowIfNull(rows);

        return sort switch
        {
            SourceSortConstants.Name => rows.OrderBy(r => r.Name).ToList(),
            SourceSortConstants.Rating => rows
                .OrderByDescending(r => r.AverageRating)
                .ThenByDescending(r => r.RecipeCount)
                .ToList(),
            // Jamais utilisée en dernier : sans date, elle n'a pas sa place parmi les
            // récentes, et la remonter en tête ferait doublon avec le raccourci dédié.
            SourceSortConstants.LastUsed => rows
                .OrderByDescending(r => r.LastUsed ?? DateTime.MinValue)
                .ThenBy(r => r.Name)
                .ToList(),
            _ => rows
                .OrderByDescending(r => r.RecipeCount)
                .ThenBy(r => r.Name)
                .ToList()
        };
    }

    /// <summary>
    /// Averages over rated recipes only. An unrated recipe holds no opinion, which is not
    /// the same as a bad one.
    /// </summary>
    private static double Average(List<Recipe> recipes)
    {
        var rated = recipes.Where(r => r.Rating > 0).ToList();
        return rated.Count == 0 ? 0 : rated.Average(r => r.Rating);
    }
}

/// <summary>Sort labels of the Sources page, kept out of the markup so both agree.</summary>
public static class SourceSortConstants
{
    public const string RecipeCount = "recipes";
    public const string Name = "name";
    public const string Rating = "rating";
    public const string LastUsed = "used";
}
