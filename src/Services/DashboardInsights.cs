using RecettesIndex.Models;

namespace RecettesIndex.Services;

/// <summary>A recipe that has been added but never rated.</summary>
public sealed record UnratedRow(int Id, string Name, string Since);

/// <summary>A book nothing has been added from in a long time.</summary>
public sealed record SleepingBookRow(int Id, string Name, int RecipeCount, string Since);

/// <summary>One bar of « de quoi est faite ma collection ».</summary>
public sealed record TagRow(int Id, string Name, int Count, double AverageRating);

/// <summary>One bar of the rating distribution. <c>Rating</c> 0 means « pas encore notée ».</summary>
public sealed record RatingRow(int Rating, int Count);

/// <summary>A book whose recipes turned out well.</summary>
public sealed record ReliableBookRow(int Id, string Name, int RecipeCount, double AverageRating);

/// <summary>Everything the dashboard shows, computed once.</summary>
public sealed record DashboardView(
    int TotalRecipes,
    int TotalSources,
    int AddedThisMonth,
    IReadOnlyList<UnratedRow> Unrated,
    int UnratedTotal,
    IReadOnlyList<SleepingBookRow> SleepingBooks,
    int SleepingTotal,
    IReadOnlyList<TagRow> Tags,
    int UntaggedCount,
    IReadOnlyList<RatingRow> Ratings,
    IReadOnlyList<ReliableBookRow> ReliableBooks);

/// <summary>
/// Turns the collection into the four questions the dashboard asks.
/// </summary>
/// <remarks>
/// Pure — it takes the recipes, the books and the shops, and returns what to draw. Every
/// figure comes from what the model already holds: the date a recipe was added, its
/// rating, its tags and its source. No new column, and in particular no « last cooked »
/// date: a fifth card needing one was proposed and dropped, and reintroducing the field
/// through the back door would be the same mistake.
/// </remarks>
public static class DashboardInsights
{
    /// <summary>A book with nothing added for this long is asleep.</summary>
    private const int SleepingAfterDays = 365;

    /// <summary>Rows kept per card. A dashboard one has to scroll answers nothing at a glance.</summary>
    private const int RowsPerCard = 5;

    /// <summary>Bars in the tag breakdown.</summary>
    private const int TagBars = 6;

    /// <summary>Books listed under « qui tiennent leurs promesses ».</summary>
    private const int ReliableBookCount = 3;

    /// <summary>
    /// A book needs more than one rated recipe before its average says anything about the
    /// book rather than about one good evening.
    /// </summary>
    private const int MinRecipesToJudgeABook = 2;

    public static DashboardView Build(
        IReadOnlyList<Recipe> recipes,
        IReadOnlyList<Book> books,
        IReadOnlyList<Store> stores,
        DateTime now)
    {
        ArgumentNullException.ThrowIfNull(recipes);
        ArgumentNullException.ThrowIfNull(books);
        ArgumentNullException.ThrowIfNull(stores);

        var unrated = recipes.Where(r => r.Rating <= 0).ToList();
        var sleeping = SleepingBooks(recipes, books, now);

        return new DashboardView(
            TotalRecipes: recipes.Count,
            TotalSources: CountSources(recipes, books, stores),
            AddedThisMonth: recipes.Count(r => r.CreationDate.Year == now.Year && r.CreationDate.Month == now.Month),
            Unrated: unrated
                .OrderByDescending(r => r.CreationDate)
                .Take(RowsPerCard)
                .Select(r => new UnratedRow(r.Id, r.Name, Since(r.CreationDate, now)))
                .ToList(),
            UnratedTotal: unrated.Count,
            SleepingBooks: sleeping.Take(RowsPerCard).ToList(),
            SleepingTotal: sleeping.Count,
            Tags: Tags(recipes).Take(TagBars).ToList(),
            UntaggedCount: recipes.Count(r => r.Etiquettes.Count == 0),
            Ratings: Ratings(recipes),
            ReliableBooks: ReliableBooks(recipes, books).Take(ReliableBookCount).ToList());
    }

    /// <summary>
    /// Books, shops and websites together — the same three the Sources page counts.
    /// </summary>
    /// <remarks>
    /// Websites are counted by distinct domain rather than by recipe: five recipes off the
    /// same site are one source, not five.
    /// </remarks>
    private static int CountSources(IReadOnlyList<Recipe> recipes, IReadOnlyList<Book> books, IReadOnlyList<Store> stores)
    {
        var domains = recipes
            .Where(r => RecipeSource.Classify(r) == RecipeSourceKind.Web)
            .Select(r => RecipeSource.Domain(r.Url))
            .Where(d => d.Length > 0)
            .Distinct()
            .Count();

        return books.Count + stores.Count + domains;
    }

    private static List<SleepingBookRow> SleepingBooks(IReadOnlyList<Recipe> recipes, IReadOnlyList<Book> books, DateTime now)
    {
        var byBook = recipes
            .Where(r => r.BookId.HasValue)
            .GroupBy(r => r.BookId!.Value)
            .ToDictionary(g => g.Key, g => (Count: g.Count(), Newest: g.Max(r => r.CreationDate)));

        // La date la plus récente est gardée à part le temps du tri : c'est elle qui
        // ordonne, pas le libellé, qu'on ne peut pas comparer.
        var rows = new List<(SleepingBookRow Row, DateTime? Newest)>();
        foreach (var book in books)
        {
            if (!byBook.TryGetValue(book.Id, out var stats))
            {
                // Un livre dont rien n'a jamais été tiré dort plus profondément encore
                // que celui délaissé depuis un an.
                rows.Add((new SleepingBookRow(book.Id, book.Name, 0, "jamais"), null));
                continue;
            }

            if ((now - stats.Newest).TotalDays >= SleepingAfterDays)
            {
                rows.Add((new SleepingBookRow(book.Id, book.Name, stats.Count, Since(stats.Newest, now)), stats.Newest));
            }
        }

        // Les plus endormis en tête : « jamais » d'abord, puis du plus ancien au plus récent.
        return rows
            .OrderBy(r => r.Newest.HasValue ? 1 : 0)
            .ThenBy(r => r.Newest ?? DateTime.MinValue)
            .ThenBy(r => r.Row.Name)
            .Select(r => r.Row)
            .ToList();
    }

    private static List<TagRow> Tags(IReadOnlyList<Recipe> recipes)
    {
        var byTag = new Dictionary<int, (string Name, int Count, int RatedCount, int RatingSum)>();

        foreach (var recipe in recipes)
        {
            foreach (var tag in recipe.Etiquettes)
            {
                byTag.TryGetValue(tag.Id, out var acc);
                byTag[tag.Id] = (
                    tag.Name,
                    acc.Count + 1,
                    acc.RatedCount + (recipe.Rating > 0 ? 1 : 0),
                    acc.RatingSum + Math.Max(0, recipe.Rating));
            }
        }

        return byTag
            .Select(kv => new TagRow(
                kv.Key,
                kv.Value.Name,
                kv.Value.Count,
                // Les non notées ne tirent pas la moyenne vers le bas : elles n'ont pas
                // d'avis, ce n'est pas un mauvais avis.
                kv.Value.RatedCount == 0 ? 0 : (double)kv.Value.RatingSum / kv.Value.RatedCount))
            .OrderByDescending(t => t.Count)
            .ThenBy(t => t.Name)
            .ToList();
    }

    /// <summary>
    /// Five down to one, then the unrated — always all six rows, so an empty bar reads as
    /// « none of those » rather than as a missing row.
    /// </summary>
    private static List<RatingRow> Ratings(IReadOnlyList<Recipe> recipes)
    {
        var rows = new List<RatingRow>();
        for (var rating = 5; rating >= 1; rating--)
        {
            rows.Add(new RatingRow(rating, recipes.Count(r => r.Rating == rating)));
        }
        rows.Add(new RatingRow(0, recipes.Count(r => r.Rating <= 0)));
        return rows;
    }

    private static List<ReliableBookRow> ReliableBooks(IReadOnlyList<Recipe> recipes, IReadOnlyList<Book> books)
    {
        var names = books.ToDictionary(b => b.Id, b => b.Name);

        return recipes
            .Where(r => r.BookId.HasValue && r.Rating > 0)
            .GroupBy(r => r.BookId!.Value)
            .Where(g => g.Count() >= MinRecipesToJudgeABook && names.ContainsKey(g.Key))
            .Select(g => new ReliableBookRow(g.Key, names[g.Key], g.Count(), g.Average(r => r.Rating)))
            .OrderByDescending(b => b.AverageRating)
            .ThenByDescending(b => b.RecipeCount)
            .ToList();
    }

    /// <summary>
    /// How long ago, in words. « il y a 3 mois » says more at a glance than a date does.
    /// </summary>
    public static string Since(DateTime from, DateTime now)
    {
        var days = (int)(now.Date - from.Date).TotalDays;

        if (days <= 0) return "aujourd'hui";
        if (days == 1) return "hier";
        if (days < 30) return $"il y a {days} jours";

        var months = days / 30;
        if (months < 12)
        {
            return months == 1 ? "il y a 1 mois" : $"il y a {months} mois";
        }

        // Plancher à un an : 364 jours donnent 12 mois par la division précédente, et
        // 364 / 365 donnerait « il y a 0 ans ».
        var years = Math.Max(1, days / 365);
        return years == 1 ? "il y a 1 an" : $"il y a {years} ans";
    }
}
