using RecettesIndex.Models;
using RecettesIndex.Services;
using Xunit;

namespace RecettesIndex.Tests.Services;

/// <summary>
/// Unit tests for DashboardInsights — the four questions the dashboard asks.
/// </summary>
public class DashboardInsightsTests
{
    private static readonly DateTime Now = new(2026, 8, 8, 12, 0, 0, DateTimeKind.Utc);

    private static Recipe MakeRecipe(
        int id,
        int rating = 0,
        int daysAgo = 0,
        int? bookId = null,
        string? url = null,
        Etiquette[]? tags = null) =>
        new()
        {
            Id = id,
            Name = $"Recette {id}",
            Rating = rating,
            BookId = bookId,
            Url = url,
            CreationDate = Now.AddDays(-daysAgo),
            Etiquettes = tags is null ? [] : [.. tags]
        };

    #region Since

    [Theory]
    [InlineData(0, "aujourd'hui")]
    [InlineData(1, "hier")]
    [InlineData(5, "il y a 5 jours")]
    [InlineData(60, "il y a 2 mois")]
    [InlineData(400, "il y a 1 an")]
    [InlineData(800, "il y a 2 ans")]
    public void Since_ReadsInWords(int daysAgo, string expected)
    {
        Assert.Equal(expected, RelativeTime.Since(Now.AddDays(-daysAgo), Now));
    }

    [Fact]
    public void Since_JustUnderAYear_NeverSaysZeroYears()
    {
        // 364 jours donnent 12 mois par la division entière, et 364 / 365 donnerait 0 :
        // sans plancher, la carte annoncerait « il y a 0 ans ».
        Assert.Equal("il y a 1 an", RelativeTime.Since(Now.AddDays(-364), Now));
    }

    #endregion

    #region Qu'est-ce qui attend une note ?

    [Fact]
    public void Unrated_CountsThemAllButListsTheFiveMostRecent()
    {
        var recipes = Enumerable.Range(1, 8).Select(i => MakeRecipe(i, rating: 0, daysAgo: i)).ToList();

        var view = DashboardInsights.Build(recipes, [], [], Now);

        Assert.Equal(8, view.UnratedTotal);
        Assert.Equal(5, view.Unrated.Count);
        Assert.Equal(1, view.Unrated[0].Id);
    }

    [Fact]
    public void Unrated_IgnoresRatedRecipes()
    {
        var recipes = new List<Recipe> { MakeRecipe(1, rating: 5), MakeRecipe(2, rating: 0) };

        var view = DashboardInsights.Build(recipes, [], [], Now);

        Assert.Equal(1, view.UnratedTotal);
        Assert.Equal(2, view.Unrated[0].Id);
    }

    #endregion

    #region Quels livres dorment ?

    [Fact]
    public void SleepingBooks_AreThoseUntouchedForAYear()
    {
        var books = new List<Book>
        {
            new() { Id = 1, Name = "Endormi" },
            new() { Id = 2, Name = "Actif" }
        };
        var recipes = new List<Recipe>
        {
            MakeRecipe(10, bookId: 1, daysAgo: 400),
            MakeRecipe(11, bookId: 2, daysAgo: 10)
        };

        var view = DashboardInsights.Build(recipes, books, [], Now);

        Assert.Single(view.SleepingBooks);
        Assert.Equal("Endormi", view.SleepingBooks[0].Name);
    }

    [Fact]
    public void SleepingBooks_ABookNothingWasEverTakenFromComesFirst()
    {
        var books = new List<Book>
        {
            new() { Id = 1, Name = "Délaissé" },
            new() { Id = 2, Name = "Jamais ouvert" }
        };
        var recipes = new List<Recipe> { MakeRecipe(10, bookId: 1, daysAgo: 400) };

        var view = DashboardInsights.Build(recipes, books, [], Now);

        Assert.Equal(2, view.SleepingBooks.Count);
        Assert.Equal("Jamais ouvert", view.SleepingBooks[0].Name);
        Assert.Equal("jamais", view.SleepingBooks[0].Since);
        Assert.Equal(0, view.SleepingBooks[0].RecipeCount);
    }

    #endregion

    #region De quoi est faite ma collection ?

    [Fact]
    public void Tags_AreOrderedByUseAndAveragedOverRatedRecipesOnly()
    {
        var souper = new Etiquette { Id = 1, Name = "Souper" };
        var dessert = new Etiquette { Id = 2, Name = "Dessert" };

        var recipes = new List<Recipe>
        {
            MakeRecipe(1, rating: 4, tags: [souper]),
            MakeRecipe(2, rating: 2, tags: [souper]),
            // Non notée : elle compte dans le total de l'étiquette, pas dans sa moyenne —
            // elle n'a pas d'avis, ce n'est pas un mauvais avis.
            MakeRecipe(3, rating: 0, tags: [souper]),
            MakeRecipe(4, rating: 5, tags: [dessert])
        };

        var view = DashboardInsights.Build(recipes, [], [], Now);

        Assert.Equal("Souper", view.Tags[0].Name);
        Assert.Equal(3, view.Tags[0].Count);
        Assert.Equal(3.0, view.Tags[0].AverageRating);
        Assert.Equal("Dessert", view.Tags[1].Name);
    }

    [Fact]
    public void UntaggedCount_IsTheFooterOfThatCard()
    {
        var tag = new Etiquette { Id = 1, Name = "Souper" };
        var recipes = new List<Recipe> { MakeRecipe(1, tags: [tag]), MakeRecipe(2), MakeRecipe(3) };

        var view = DashboardInsights.Build(recipes, [], [], Now);

        Assert.Equal(2, view.UntaggedCount);
    }

    #endregion

    #region Est-ce que je note honnêtement ?

    [Fact]
    public void Ratings_AlwaysHaveTheSixRowsEvenWhenEmpty()
    {
        var view = DashboardInsights.Build([MakeRecipe(1, rating: 5)], [], [], Now);

        // Cinq à un, puis les non notées : une barre vide dit « aucune », une ligne
        // manquante ne dit rien du tout.
        Assert.Equal(6, view.Ratings.Count);
        Assert.Equal(new[] { 5, 4, 3, 2, 1, 0 }, view.Ratings.Select(r => r.Rating).ToArray());
        Assert.Equal(1, view.Ratings[0].Count);
        Assert.Equal(0, view.Ratings[5].Count);
    }

    [Fact]
    public void ReliableBooks_NeedMoreThanOneRatedRecipe()
    {
        var books = new List<Book>
        {
            new() { Id = 1, Name = "Un seul essai" },
            new() { Id = 2, Name = "Tient ses promesses" }
        };
        var recipes = new List<Recipe>
        {
            // Une seule recette notée 5 ne dit rien du livre, seulement d'un bon soir.
            MakeRecipe(10, rating: 5, bookId: 1),
            MakeRecipe(20, rating: 4, bookId: 2),
            MakeRecipe(21, rating: 5, bookId: 2)
        };

        var view = DashboardInsights.Build(recipes, books, [], Now);

        Assert.Single(view.ReliableBooks);
        Assert.Equal("Tient ses promesses", view.ReliableBooks[0].Name);
        Assert.Equal(4.5, view.ReliableBooks[0].AverageRating);
    }

    #endregion

    #region En-tête

    [Fact]
    public void SourceCount_CountsASiteOnceHoweverManyRecipesComeFromIt()
    {
        var recipes = new List<Recipe>
        {
            MakeRecipe(1, url: "https://ricardocuisine.com/a"),
            MakeRecipe(2, url: "https://www.ricardocuisine.com/b"),
            MakeRecipe(3, url: "https://autre.com/c")
        };
        var books = new List<Book> { new() { Id = 1, Name = "Un livre" } };
        var stores = new List<Store> { new() { Id = 1, Name = "Un commerce" } };

        var view = DashboardInsights.Build(recipes, books, stores, Now);

        // 1 livre + 1 commerce + 2 domaines distincts.
        Assert.Equal(4, view.TotalSources);
    }

    [Fact]
    public void AddedThisMonth_CountsTheCalendarMonth()
    {
        var recipes = new List<Recipe>
        {
            MakeRecipe(1, daysAgo: 2),
            MakeRecipe(2, daysAgo: 40),
            MakeRecipe(3, daysAgo: 400)
        };

        var view = DashboardInsights.Build(recipes, [], [], Now);

        Assert.Equal(3, view.TotalRecipes);
        Assert.Equal(1, view.AddedThisMonth);
    }

    #endregion
}
