using RecettesIndex.Models;
using RecettesIndex.Services;
using Xunit;

namespace RecettesIndex.Tests.Services;

public class RecipeSearchServiceTests
{
    private static readonly Author Michalak = new() { Id = 1, Name = "Christophe", LastName = "Michalak" };

    private static readonly Book Patisserie = new()
    {
        Id = 1,
        Name = "Pâtisserie maison",
        Authors = [Michalak]
    };

    private static readonly Book Vietnam = new() { Id = 2, Name = "Saveurs du Vietnam" };

    private static readonly Store Rotisserie = new() { Id = 1, Name = "Rôtisserie St-Roch" };

    private static readonly List<Book> Books = [Patisserie, Vietnam];
    private static readonly List<Store> Stores = [Rotisserie];

    private static List<Recipe> Recipes() =>
    [
        new() { Id = 1, Name = "Tarte au sucre", BookId = 1, BookPage = 84, Rating = 5 },
        new() { Id = 2, Name = "Crème brûlée", BookId = 1, BookPage = 32, Rating = 4 },
        new() { Id = 3, Name = "Soupe tonkinoise", BookId = 2, BookPage = 56, Rating = 3 },
        new() { Id = 4, Name = "Poutine du vendredi", StoreId = 1, Rating = 2,
                Notes = "Demander la sauce à part." },
        new() { Id = 5, Name = "Salade verte", Rating = 0,
                Etiquettes = [new Etiquette { Id = 1, Name = "Végé" }] }
    ];

    [Fact]
    public void SearchRecipes_EmptyQuery_ReturnsNothing()
    {
        Assert.Empty(RecipeSearchService.SearchRecipes("", Recipes(), Books, Stores));
        Assert.Empty(RecipeSearchService.SearchRecipes("   ", Recipes(), Books, Stores));
        Assert.Empty(RecipeSearchService.SearchRecipes(null, Recipes(), Books, Stores));
    }

    [Fact]
    public void SearchRecipes_IgnoresAccents()
    {
        // « creme » sans accent doit trouver « Crème brûlée ».
        var hits = RecipeSearchService.SearchRecipes("creme", Recipes(), Books, Stores);

        Assert.Contains(hits, h => h.Label == "Crème brûlée");
    }

    [Fact]
    public void SearchRecipes_ToleratesTypos()
    {
        // Le cas cité par le design : « tart au sucr » doit trouver « Tarte au sucre ».
        var hits = RecipeSearchService.SearchRecipes("tarte au sucr", Recipes(), Books, Stores);

        Assert.Contains(hits, h => h.Label == "Tarte au sucre");
    }

    [Fact]
    public void SearchRecipes_MatchesOnBookTitle()
    {
        var hits = RecipeSearchService.SearchRecipes("vietnam", Recipes(), Books, Stores);

        Assert.Contains(hits, h => h.Label == "Soupe tonkinoise");
    }

    [Fact]
    public void SearchRecipes_MatchesOnAuthor()
    {
        var hits = RecipeSearchService.SearchRecipes("michalak", Recipes(), Books, Stores);

        Assert.Contains(hits, h => h.Label == "Tarte au sucre");
        Assert.Contains(hits, h => h.Label == "Crème brûlée");
    }

    [Fact]
    public void SearchRecipes_MatchesOnStore()
    {
        var hits = RecipeSearchService.SearchRecipes("rotisserie", Recipes(), Books, Stores);

        Assert.Contains(hits, h => h.Label == "Poutine du vendredi");
    }

    [Fact]
    public void SearchRecipes_MatchesOnTag()
    {
        var hits = RecipeSearchService.SearchRecipes("vege", Recipes(), Books, Stores);

        Assert.Contains(hits, h => h.Label == "Salade verte");
    }

    [Fact]
    public void SearchRecipes_MatchesOnNotes()
    {
        var hits = RecipeSearchService.SearchRecipes("sauce", Recipes(), Books, Stores);

        Assert.Contains(hits, h => h.Label == "Poutine du vendredi");
    }

    [Fact]
    public void SearchRecipes_RanksNameAboveNotes()
    {
        var recipes = new List<Recipe>
        {
            new() { Id = 10, Name = "Gâteau au chocolat" },
            new() { Id = 11, Name = "Biscuits", Notes = "Ajouter du chocolat." }
        };

        var hits = RecipeSearchService.SearchRecipes("chocolat", recipes, Books, Stores);

        // La priorité du README : le nom l'emporte sur les notes.
        Assert.Equal("Gâteau au chocolat", hits[0].Label);
    }

    [Fact]
    public void SearchRecipes_RanksExactNameAbovePrefix()
    {
        var recipes = new List<Recipe>
        {
            new() { Id = 20, Name = "Tarte aux pommes" },
            new() { Id = 21, Name = "Tarte" }
        };

        var hits = RecipeSearchService.SearchRecipes("tarte", recipes, Books, Stores);

        Assert.Equal("Tarte", hits[0].Label);
    }

    [Fact]
    public void SearchRecipes_CarriesPageAndSourceForTheRow()
    {
        var hit = RecipeSearchService.SearchRecipes("tarte au sucre", Recipes(), Books, Stores).First();

        // La palette répond « c'était où ? » sans qu'on ouvre la fiche.
        Assert.Equal(84, hit.Page);
        Assert.Equal("Pâtisserie maison", hit.Secondary);
        Assert.Equal(5, hit.Rating);
    }

    [Fact]
    public void SearchRecipes_HonoursTheMaximum()
    {
        var many = Enumerable.Range(1, 20)
            .Select(i => new Recipe { Id = i, Name = $"Tarte {i}" })
            .ToList();

        var hits = RecipeSearchService.SearchRecipes("tarte", many, Books, Stores, max: 4);

        Assert.Equal(4, hits.Count);
    }

    [Fact]
    public void SearchBooks_MatchesTitleAndReportsRecipeCount()
    {
        var hits = RecipeSearchService.SearchBooks("patisserie", Books, Recipes());

        var hit = Assert.Single(hits);
        Assert.Equal("Pâtisserie maison", hit.Label);
        Assert.Equal(SearchHitKind.Book, hit.Kind);
        Assert.Contains("2 recettes", hit.Secondary);
        Assert.Contains("Christophe Michalak", hit.Secondary);
    }

    [Fact]
    public void SearchBooks_SingleRecipeIsNotPluralised()
    {
        var hits = RecipeSearchService.SearchBooks("vietnam", Books, Recipes());

        Assert.Contains("1 recette", Assert.Single(hits).Secondary);
    }

    [Fact]
    public void SearchBooks_EmptyQueryReturnsNothing()
    {
        Assert.Empty(RecipeSearchService.SearchBooks("", Books, Recipes()));
    }
}
