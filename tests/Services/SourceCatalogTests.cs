using RecettesIndex.Models;
using RecettesIndex.Services;
using Xunit;

namespace RecettesIndex.Tests.Services;

/// <summary>
/// Unit tests for SourceCatalog — the single list replacing Books, Stores and Authors.
/// </summary>
public class SourceCatalogTests
{
    private static readonly DateTime Now = new(2026, 8, 8, 12, 0, 0, DateTimeKind.Utc);

    private static Recipe MakeRecipe(int id, int rating = 0, int daysAgo = 0, int? bookId = null, int? storeId = null, string? url = null) =>
        new()
        {
            Id = id,
            Name = $"Recette {id}",
            Rating = rating,
            BookId = bookId,
            StoreId = storeId,
            Url = url,
            CreationDate = Now.AddDays(-daysAgo)
        };

    private static Book MakeBook(int id, string name, params Author[] authors) =>
        new() { Id = id, Name = name, Authors = [.. authors] };

    #region Build

    [Fact]
    public void Build_GivesEveryBookARowEvenWithNoRecipes()
    {
        // C'est tout l'intérêt de la page : rendre visible ce qu'on n'utilise jamais.
        var rows = SourceCatalog.Build([], [MakeBook(1, "Jamais ouvert")], []);

        Assert.Single(rows);
        Assert.Equal(0, rows[0].RecipeCount);
        Assert.Null(rows[0].LastUsed);
    }

    [Fact]
    public void Build_CountsASiteOnceHoweverManyRecipesComeFromIt()
    {
        var recipes = new List<Recipe>
        {
            MakeRecipe(1, url: "https://ricardocuisine.com/a"),
            MakeRecipe(2, url: "https://www.ricardocuisine.com/b")
        };

        var rows = SourceCatalog.Build(recipes, [], []);

        var site = Assert.Single(rows);
        Assert.Equal(RecipeSourceKind.Web, site.Kind);
        Assert.Equal("ricardocuisine.com", site.Name);
        Assert.Equal(2, site.RecipeCount);
    }

    [Fact]
    public void Build_AveragesOverRatedRecipesOnly()
    {
        var recipes = new List<Recipe>
        {
            MakeRecipe(1, rating: 4, bookId: 7),
            MakeRecipe(2, rating: 2, bookId: 7),
            // Non notée : elle compte dans le total, pas dans la moyenne.
            MakeRecipe(3, rating: 0, bookId: 7)
        };

        var rows = SourceCatalog.Build(recipes, [MakeBook(7, "Un livre")], []);

        Assert.Equal(3, rows[0].RecipeCount);
        Assert.Equal(3.0, rows[0].AverageRating);
    }

    [Fact]
    public void Build_PutsTheAuthorInTheSubtitleOfABook()
    {
        var author = new Author { Id = 1, Name = "Meera", LastName = "Sodha" };

        var rows = SourceCatalog.Build([], [MakeBook(7, "Made in India", author)], []);

        Assert.Equal("Meera Sodha", rows[0].Subtitle);
    }

    #endregion

    #region Count

    [Fact]
    public void Count_SplitsTheThreeKindsAndTheNeverUsed()
    {
        var recipes = new List<Recipe>
        {
            MakeRecipe(1, bookId: 1),
            MakeRecipe(2, storeId: 1),
            MakeRecipe(3, url: "https://exemple.com/a")
        };
        var books = new List<Book> { MakeBook(1, "Utilisé"), MakeBook(2, "Jamais ouvert") };
        var stores = new List<Store> { new() { Id = 1, Name = "Le Marché" } };

        var counts = SourceCatalog.Count(SourceCatalog.Build(recipes, books, stores));

        Assert.Equal(4, counts.All);
        Assert.Equal(2, counts.Books);
        Assert.Equal(1, counts.Stores);
        Assert.Equal(1, counts.Sites);
        Assert.Equal(1, counts.NeverUsed);
    }

    #endregion

    #region Filter

    [Fact]
    public void Filter_ByAuthor_IsHowAuthorsSurvive()
    {
        // Un auteur n'a pas de contenu propre : il devient un filtre, pas une page.
        var meera = new Author { Id = 1, Name = "Meera", LastName = "Sodha" };
        var other = new Author { Id = 2, Name = "Ricardo", LastName = "Larrivée" };
        var rows = SourceCatalog.Build([], [MakeBook(7, "Made in India", meera), MakeBook(8, "Autre", other)], []);

        var filtered = SourceCatalog.Filter(rows, authorId: 1);

        Assert.Single(filtered);
        Assert.Equal("Made in India", filtered[0].Name);
    }

    [Fact]
    public void Filter_ByText_ToleratesMissingAccents()
    {
        var rows = SourceCatalog.Build([], [MakeBook(7, "Crème et compagnie")], []);

        Assert.Single(SourceCatalog.Filter(rows, text: "creme"));
    }

    [Fact]
    public void Filter_ByText_AlsoSearchesTheSubtitle()
    {
        // On cherche un livre par son auteur au moins aussi souvent que par son titre.
        var author = new Author { Id = 1, Name = "Meera", LastName = "Sodha" };
        var rows = SourceCatalog.Build([], [MakeBook(7, "Made in India", author)], []);

        Assert.Single(SourceCatalog.Filter(rows, text: "sodha"));
    }

    [Fact]
    public void Filter_ByKindAndNeverUsed_Combine()
    {
        var recipes = new List<Recipe> { MakeRecipe(1, bookId: 1) };
        var books = new List<Book> { MakeBook(1, "Utilisé"), MakeBook(2, "Jamais ouvert") };
        var stores = new List<Store> { new() { Id = 1, Name = "Jamais servi" } };
        var rows = SourceCatalog.Build(recipes, books, stores);

        var filtered = SourceCatalog.Filter(rows, kind: RecipeSourceKind.Book, neverUsedOnly: true);

        Assert.Single(filtered);
        Assert.Equal("Jamais ouvert", filtered[0].Name);
    }

    #endregion

    #region Sort

    [Fact]
    public void Sort_DefaultsToTheRecipeCount()
    {
        var recipes = new List<Recipe> { MakeRecipe(1, bookId: 2), MakeRecipe(2, bookId: 2), MakeRecipe(3, bookId: 1) };
        var rows = SourceCatalog.Build(recipes, [MakeBook(1, "Un"), MakeBook(2, "Deux")], []);

        var sorted = SourceCatalog.Sort(rows, null);

        Assert.Equal("Deux", sorted[0].Name);
    }

    [Fact]
    public void Sort_ByLastUsed_LeavesTheNeverUsedAtTheBottom()
    {
        // Sans date, elles n'ont pas leur place parmi les récentes — et le raccourci
        // « jamais ouvertes » existe déjà pour les trouver.
        var recipes = new List<Recipe> { MakeRecipe(1, bookId: 1, daysAgo: 5) };
        var rows = SourceCatalog.Build(recipes, [MakeBook(1, "Récent"), MakeBook(2, "Jamais")], []);

        var sorted = SourceCatalog.Sort(rows, SourceSortConstants.LastUsed);

        Assert.Equal("Récent", sorted[0].Name);
        Assert.Equal("Jamais", sorted[1].Name);
    }

    #endregion
}
