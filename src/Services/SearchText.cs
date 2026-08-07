using System.Globalization;
using System.Text;

namespace RecettesIndex.Services;

/// <summary>
/// Text primitives behind the instant search: normalisation and fuzzy comparison.
/// </summary>
/// <remarks>
/// Deliberately free of any dependency, so it can be reasoned about and tested on its
/// own. The collection is small — tens to hundreds of recipes — so everything runs
/// client-side with no network round trip.
/// </remarks>
public static class SearchText
{
    /// <summary>
    /// Folds a string to its comparison form: lower case, no diacritics, single spaces.
    /// </summary>
    /// <remarks>
    /// This is what lets "creme" find « Crème » and "tarte" find « Tarte ». Typing
    /// accents on a phone is enough friction to lose the search entirely.
    /// </remarks>
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var decomposed = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        var lastWasSpace = false;

        foreach (var c in decomposed)
        {
            // FormD splits "é" into "e" + combining accent; dropping the accent leaves "e".
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsWhiteSpace(c))
            {
                if (!lastWasSpace && builder.Length > 0)
                {
                    builder.Append(' ');
                }
                lastWasSpace = true;
                continue;
            }

            lastWasSpace = false;
            builder.Append(c);
        }

        return builder.ToString().TrimEnd().Normalize(NormalizationForm.FormC);
    }

    /// <summary>
    /// Levenshtein distance, capped: it stops as soon as the distance exceeds
    /// <paramref name="max"/> and returns <paramref name="max"/> + 1.
    /// </summary>
    /// <remarks>
    /// The cap matters because callers only ever ask "is this within 2?" — computing the
    /// exact distance between two long unrelated words is wasted work on every keystroke.
    /// </remarks>
    public static int Levenshtein(string a, string b, int max = int.MaxValue)
    {
        if (a == b) return 0;
        if (a.Length == 0) return b.Length;
        if (b.Length == 0) return a.Length;
        if (Math.Abs(a.Length - b.Length) > max) return max + 1;

        // Two rows are enough: the algorithm never looks further back than the previous one.
        var previous = new int[b.Length + 1];
        var current = new int[b.Length + 1];

        for (var j = 0; j <= b.Length; j++)
        {
            previous[j] = j;
        }

        for (var i = 1; i <= a.Length; i++)
        {
            current[0] = i;
            var rowMin = current[0];

            for (var j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                current[j] = Math.Min(
                    Math.Min(current[j - 1] + 1, previous[j] + 1),
                    previous[j - 1] + cost);
                rowMin = Math.Min(rowMin, current[j]);
            }

            if (rowMin > max)
            {
                return max + 1;
            }

            (previous, current) = (current, previous);
        }

        return previous[b.Length];
    }

    /// <summary>
    /// Whether a normalised haystack contains a term, allowing prefixes and typos.
    /// </summary>
    /// <param name="haystack">Already normalised text to search in.</param>
    /// <param name="term">Already normalised single term.</param>
    /// <remarks>
    /// Typo tolerance only applies from five letters up. Below that, a distance of two
    /// turns "the" into half the dictionary and the results stop making sense.
    /// </remarks>
    public static bool ContainsTerm(string haystack, string term)
    {
        if (term.Length == 0) return true;
        if (haystack.Length == 0) return false;

        if (haystack.Contains(term, StringComparison.Ordinal))
        {
            return true;
        }

        if (term.Length <= 4)
        {
            return false;
        }

        foreach (var word in haystack.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (Levenshtein(word, term, 2) <= 2)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Splits a normalised query into its terms.</summary>
    public static string[] Terms(string normalizedQuery)
        => normalizedQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);
}
