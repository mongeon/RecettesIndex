using System.Diagnostics.CodeAnalysis;
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
    /// Anything unparseable is handed back whole rather than dropped — showing a strange
    /// title beats showing nothing where the source belongs.
    /// </remarks>
    public static (string Domain, string Path) SplitUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return (string.Empty, string.Empty);
        }

        var trimmed = url.Trim();
        if (!TryParse(trimmed, out var uri))
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
    /// Returns the stored URL in a form safe to put in an <c>href</c>, or null when there is
    /// nothing safe to link to.
    /// </summary>
    /// <remarks>
    /// Two reasons this cannot be the raw column value. A URL stored without a scheme —
    /// « ricardocuisine.com/… », which is exactly what pasting from an address bar produces —
    /// is read by the browser as a path relative to the app, so the link would land back on
    /// our own 404 instead of the recipe. And a stored <c>javascript:</c> URL would otherwise
    /// become a clickable script; only http and https get through here.
    /// </remarks>
    public static string? ExternalHref(string? url)
    {
        if (!TryParse(url, out var uri))
        {
            return null;
        }

        return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps
            ? uri.AbsoluteUri
            : null;
    }

    /// <summary>
    /// Parses a stored URL the way the whole page reads it, so the title, the domain
    /// comparison and the link can never disagree about what the string means.
    /// </summary>
    /// <remarks>
    /// A link pasted from an address bar often arrives without a scheme, which
    /// <see cref="Uri"/> refuses to read as absolute; one is added before parsing. Inner
    /// whitespace disqualifies the string first, because the parser would otherwise accept
    /// free text as a host.
    /// </remarks>
    private static bool TryParse(string? url, [NotNullWhen(true)] out Uri? uri)
    {
        uri = null;

        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        var trimmed = url.Trim();
        if (trimmed.Any(char.IsWhiteSpace))
        {
            return false;
        }

        // Le crible « :// » ne porte aucune lettre : la casse du schéma n'entre pas en
        // jeu, HTTPS://exemple.com est reconnu comme ayant déjà son schéma.
        var candidate = trimmed.Contains("://", StringComparison.Ordinal) ? trimmed : $"https://{trimmed}";

        return Uri.TryCreate(candidate, UriKind.Absolute, out uri) && !string.IsNullOrEmpty(uri.Host);
    }

    /// <summary>
    /// Returns the domain of a URL, lowercased, for comparing two recipes' sites.
    /// </summary>
    public static string Domain(string? url) => SplitUrl(url).Domain.ToLowerInvariant();
}
