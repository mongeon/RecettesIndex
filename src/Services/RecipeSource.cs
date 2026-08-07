using RecettesIndex.Models;

namespace RecettesIndex.Services;

/// <summary>
/// Where a recipe is to be found — the four cases the detail page has to answer.
/// </summary>
public enum RecipeSourceKind
{
    /// <summary>A cookbook on the shelf, with a page number.</summary>
    Book,

    /// <summary>A shop or a restaurant that sells it ready-made.</summary>
    Store,

    /// <summary>A page somewhere on the web.</summary>
    Web,

    /// <summary>Nowhere else: the recipe is the household's own.</summary>
    Home
}

/// <summary>
/// Reads a recipe's source. Pure — no database, no rendering — so the branching that
/// drives the whole detail page can be tested on its own.
/// </summary>
public static class RecipeSource
{
    /// <summary>
    /// Decides which of the four sources a recipe comes from.
    /// </summary>
    /// <remarks>
    /// The order is a priority, not a set of exclusive cases: a recipe can carry both a
    /// book and a URL, and the book wins because it is the more precise answer to
    /// "where do I find it?".
    /// </remarks>
    public static RecipeSourceKind Classify(Recipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        if (recipe.BookId.HasValue)
        {
            return RecipeSourceKind.Book;
        }

        if (recipe.StoreId.HasValue)
        {
            return RecipeSourceKind.Store;
        }

        return recipe.IsFromUrl ? RecipeSourceKind.Web : RecipeSourceKind.Home;
    }

    /// <summary>
    /// Splits a stored URL into the domain shown as the title and the path shown under it.
    /// </summary>
    /// <returns>
    /// The domain without its <c>www.</c> prefix, and the path — empty when the link points
    /// at the site root. Both are empty for a blank URL.
    /// </returns>
    /// <remarks>
    /// A link pasted from an address bar often arrives without a scheme, which
    /// <see cref="Uri"/> refuses to read as absolute; one is added before parsing. Inner
    /// whitespace disqualifies the string first, because the parser would otherwise accept
    /// free text as a host. Anything unparseable is handed back whole rather than dropped —
    /// showing a strange title beats showing nothing where the source belongs.
    /// </remarks>
    public static (string Domain, string Path) SplitUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return (string.Empty, string.Empty);
        }

        var trimmed = url.Trim();
        var candidate = trimmed.Contains("://", StringComparison.Ordinal) ? trimmed : $"https://{trimmed}";

        if (trimmed.Any(char.IsWhiteSpace)
            || !Uri.TryCreate(candidate, UriKind.Absolute, out var uri)
            || string.IsNullOrEmpty(uri.Host))
        {
            return (trimmed, string.Empty);
        }

        var domain = uri.Host.StartsWith("www.", StringComparison.OrdinalIgnoreCase)
            ? uri.Host[4..]
            : uri.Host;

        var path = uri.PathAndQuery;
        if (path == "/")
        {
            path = string.Empty;
        }

        return (domain, path);
    }

    /// <summary>
    /// Returns the domain of a URL, lowercased, for comparing two recipes' sites.
    /// </summary>
    public static string Domain(string? url) => SplitUrl(url).Domain.ToLowerInvariant();
}
