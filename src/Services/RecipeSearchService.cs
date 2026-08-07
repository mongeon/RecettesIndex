using RecettesIndex.Models;

namespace RecettesIndex.Services;

/// <summary>What a palette row points at.</summary>
public enum SearchHitKind
{
    Recipe,
    Book
}

/// <summary>One row of the instant-search palette.</summary>
/// <param name="Kind">Recipe or book.</param>
/// <param name="Id">Identifier of the target entity.</param>
/// <param name="Label">Primary text — the recipe or book name.</param>
/// <param name="Secondary">Right-hand context: the source, or "3 recettes · Auteur".</param>
/// <param name="Page">Book page, so the palette can answer "where was it?" without opening anything.</param>
/// <param name="Rating">Recipe rating, rendered as pizzas; zero for books.</param>
/// <param name="Score">Relevance; higher sorts first.</param>
public sealed record SearchHit(
    SearchHitKind Kind,
    int Id,
    string Label,
    string? Secondary,
    int? Page,
    int Rating,
    int Score);

/// <summary>
/// Ranks recipes and books against a free-text query, entirely in memory.
/// </summary>
/// <remarks>
/// No network call and no injected dependency: the collection is small enough that the
/// whole thing runs on each keystroke, and staying pure keeps it testable without a
/// database. Scores follow the priority the design asks for — an exact name beats a
/// prefix, which beats a word, which beats a book or author, which beats the notes.
/// </remarks>
public static class RecipeSearchService
{
    private const int ScoreExactName = 100;
    private const int ScoreNamePrefix = 80;
    private const int ScoreNameWord = 60;
    private const int ScoreNameFuzzy = 45;
    private const int ScoreSource = 30;
    private const int ScoreTag = 25;
    private const int ScoreNotes = 10;

    /// <summary>
    /// Returns the best recipe matches, most relevant first.
    /// </summary>
    /// <param name="query">Raw user input; normalised internally.</param>
    /// <param name="recipes">Candidate recipes, tags included.</param>
    /// <param name="books">Books, used to match on title and author.</param>
    /// <param name="stores">Stores, used to match on name.</param>
    /// <param name="max">Maximum number of rows to return.</param>
    public static IReadOnlyList<SearchHit> SearchRecipes(
        string? query,
        IEnumerable<Recipe> recipes,
        IReadOnlyCollection<Book> books,
        IReadOnlyCollection<Store> stores,
        int max = 6)
    {
        var normalized = SearchText.Normalize(query);
        if (normalized.Length == 0)
        {
            return [];
        }

        var terms = SearchText.Terms(normalized);
        var hits = new List<SearchHit>();

        foreach (var recipe in recipes)
        {
            var score = ScoreRecipe(recipe, normalized, terms, books, stores);
            if (score <= 0)
            {
                continue;
            }

            var book = recipe.BookId is int bookId ? books.FirstOrDefault(b => b.Id == bookId) : null;
            var store = recipe.StoreId is int storeId ? stores.FirstOrDefault(s => s.Id == storeId) : null;

            hits.Add(new SearchHit(
                SearchHitKind.Recipe,
                recipe.Id,
                recipe.Name,
                book?.Name ?? store?.Name ?? (recipe.IsFromUrl ? "Site web" : "Maison"),
                recipe.BookPage,
                recipe.Rating,
                score));
        }

        return hits
            .OrderByDescending(h => h.Score)
            .ThenBy(h => h.Label, StringComparer.CurrentCultureIgnoreCase)
            .Take(max)
            .ToList();
    }

    /// <summary>
    /// Returns the best book matches, with their recipe count and authors as context.
    /// </summary>
    public static IReadOnlyList<SearchHit> SearchBooks(
        string? query,
        IReadOnlyCollection<Book> books,
        IReadOnlyCollection<Recipe> recipes,
        int max = 3)
    {
        var normalized = SearchText.Normalize(query);
        if (normalized.Length == 0)
        {
            return [];
        }

        var terms = SearchText.Terms(normalized);
        var hits = new List<SearchHit>();

        foreach (var book in books)
        {
            var title = SearchText.Normalize(book.Name);
            var authors = SearchText.Normalize(string.Join(' ', book.Authors.Select(a => a.FullName)));

            var score =
                title == normalized ? ScoreExactName
                : title.StartsWith(normalized, StringComparison.Ordinal) ? ScoreNamePrefix
                : terms.All(t => SearchText.ContainsTerm(title, t)) ? ScoreNameWord
                : terms.All(t => SearchText.ContainsTerm(authors, t)) ? ScoreSource
                : 0;

            if (score <= 0)
            {
                continue;
            }

            var count = recipes.Count(r => r.BookId == book.Id);
            var authorNames = string.Join(", ", book.Authors.Select(a => a.FullName));
            var secondary = count == 1 ? "1 recette" : $"{count} recettes";
            if (!string.IsNullOrWhiteSpace(authorNames))
            {
                secondary += $" · {authorNames}";
            }

            hits.Add(new SearchHit(SearchHitKind.Book, book.Id, book.Name, secondary, null, 0, score));
        }

        return hits
            .OrderByDescending(h => h.Score)
            .ThenBy(h => h.Label, StringComparer.CurrentCultureIgnoreCase)
            .Take(max)
            .ToList();
    }

    private static int ScoreRecipe(
        Recipe recipe,
        string normalizedQuery,
        string[] terms,
        IReadOnlyCollection<Book> books,
        IReadOnlyCollection<Store> stores)
    {
        var name = SearchText.Normalize(recipe.Name);

        if (name == normalizedQuery)
        {
            return ScoreExactName;
        }

        if (name.StartsWith(normalizedQuery, StringComparison.Ordinal))
        {
            return ScoreNamePrefix;
        }

        // Every term must land somewhere in the name — "tarte sucre" should not match a
        // recipe that only has "tarte", or the ranking stops meaning anything.
        if (terms.All(t => name.Contains(t, StringComparison.Ordinal)))
        {
            return ScoreNameWord;
        }

        if (terms.All(t => SearchText.ContainsTerm(name, t)))
        {
            return ScoreNameFuzzy;
        }

        var book = recipe.BookId is int bookId ? books.FirstOrDefault(b => b.Id == bookId) : null;
        var store = recipe.StoreId is int storeId ? stores.FirstOrDefault(s => s.Id == storeId) : null;
        var source = SearchText.Normalize(string.Join(' ', new[]
        {
            book?.Name,
            book is null ? null : string.Join(' ', book.Authors.Select(a => a.FullName)),
            store?.Name
        }.Where(s => !string.IsNullOrWhiteSpace(s))));

        if (source.Length > 0 && terms.All(t => SearchText.ContainsTerm(source, t)))
        {
            return ScoreSource;
        }

        var tags = SearchText.Normalize(string.Join(' ', recipe.Etiquettes.Select(e => e.Name)));
        if (tags.Length > 0 && terms.All(t => SearchText.ContainsTerm(tags, t)))
        {
            return ScoreTag;
        }

        var notes = SearchText.Normalize(recipe.Notes);
        if (notes.Length > 0 && terms.All(t => SearchText.ContainsTerm(notes, t)))
        {
            return ScoreNotes;
        }

        return 0;
    }
}
