using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor.Services;
using NSubstitute;
using RecettesIndex.Models;
using RecettesIndex.Pages;
using RecettesIndex.Services;
using RecettesIndex.Services.Abstractions;

namespace RecettesIndex.Tests.Pages;

public class RecipeDetailsPageTests : BunitContext
{
    private readonly IRecipeService _recipeService = Substitute.For<IRecipeService>();

    public RecipeDetailsPageTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        var jsRuntime = Substitute.For<IJSRuntime>();

        Services.AddMudServices();
        Services.AddSingleton(jsRuntime);
        Services.AddSingleton(_recipeService);
        Services.AddSingleton(Substitute.For<ILogger<RecipeDetails>>());

        var authWrapper = Substitute.For<ISupabaseAuthWrapper>();
        Services.AddSingleton(new AuthService(authWrapper));

        Services.AddSingleton(Substitute.For<ILocalStorageService>());
    }

    #region Helpers

    /// <summary>
    /// Makes every query the source banner and the side panel run answer with nothing,
    /// so a test only has to set up the one call it is actually about.
    /// </summary>
    private void StubEmptyNeighbours()
    {
        _recipeService.SearchAsync(
                Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(),
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<bool>(),
                Arg.Any<IReadOnlyCollection<int>?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<(IReadOnlyList<Recipe> Items, int Total)>.Success((Array.Empty<Recipe>(), 0))));

        _recipeService.GetRecipeSummariesAsync(Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IReadOnlyList<Recipe>>.Success(Array.Empty<Recipe>())));

        _recipeService.GetRecipesByIdsAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IReadOnlyList<Recipe>>.Success(Array.Empty<Recipe>())));
    }

    private void StubRecipe(Recipe recipe)
    {
        _recipeService.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Recipe>.Success(recipe)));
    }

    /// <summary>Stubs the whole page for a recipe whose source has no neighbours.</summary>
    private void ArrangeAlone(Recipe recipe)
    {
        StubEmptyNeighbours();
        StubRecipe(recipe);
    }

    #endregion

    #region Empty and error states

    [Fact]
    public void RendersNotFoundEmptyState_WhenRecipeDoesNotExist()
    {
        StubEmptyNeighbours();
        _recipeService.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Recipe>.Failure("Recipe with ID 999 not found")));

        var cut = Render<RecipeDetails>(parameters => parameters.Add(p => p.Id, 999));

        cut.WaitForAssertion(() =>
            Assert.Contains("Recette introuvable", cut.Markup));
    }

    [Fact]
    public void RendersErrorAlert_WhenLoadingThrowsUnexpectedException()
    {
        _recipeService.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromException<Result<Recipe>>(new Exception("Unexpected failure")));

        var cut = Render<RecipeDetails>(parameters => parameters.Add(p => p.Id, 1));

        cut.WaitForAssertion(() =>
            Assert.Contains("Impossible de charger cette recette pour le moment", cut.Markup));
    }

    #endregion

    #region Le bandeau de source, ses quatre cas

    [Fact]
    public void BookRecipe_ShowsBookBannerWithPageNumber()
    {
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Crème brûlée",
            BookId = 7,
            BookPage = 62,
            Book = new Book { Id = 7, Name = "Made in India" }
        };

        ArrangeAlone(recipe);

        var cut = Render<RecipeDetails>(parameters => parameters.Add(p => p.Id, recipe.Id));

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Dans quel livre", cut.Markup);
            Assert.Contains("Made in India", cut.Markup);
            Assert.Contains(">62</div>", cut.Markup);
        });
    }

    [Fact]
    public void BookRecipeWithoutPage_ShowsEmDashRatherThanAGap()
    {
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Dal",
            BookId = 7,
            Book = new Book { Id = 7, Name = "Made in India" }
        };

        ArrangeAlone(recipe);

        var cut = Render<RecipeDetails>(parameters => parameters.Add(p => p.Id, recipe.Id));

        cut.WaitForAssertion(() => Assert.Contains(">—</div>", cut.Markup));
    }

    [Fact]
    public void StoreRecipe_ShowsStoreBannerAndLinksToTheStore()
    {
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Pâté chinois",
            StoreId = 3,
            Store = new Store { Id = 3, Name = "Le Marché" }
        };

        ArrangeAlone(recipe);

        var cut = Render<RecipeDetails>(parameters => parameters.Add(p => p.Id, recipe.Id));

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Où on l'achète", cut.Markup);
            Assert.Contains("Le Marché", cut.Markup);
            Assert.Contains("href=\"/stores/3\"", cut.Markup);
        });
    }

    [Fact]
    public void WebRecipe_ShowsDomainAsTitleAndPathBelow()
    {
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Tarte au sucre",
            Url = "https://www.ricardocuisine.com/recettes/123"
        };

        ArrangeAlone(recipe);

        var cut = Render<RecipeDetails>(parameters => parameters.Add(p => p.Id, recipe.Id));

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Trouvée en ligne", cut.Markup);
            Assert.Contains("ricardocuisine.com", cut.Markup);
            // Le chemin est bien le sous-titre, pas seulement le href du bouton.
            Assert.Contains(">/recettes/123</div>", cut.Markup);
        });
    }

    [Fact]
    public void WebRecipeStoredWithoutScheme_StillLinksOutside()
    {
        // Tel quel, « ricardocuisine.com/… » serait lu par le navigateur comme un chemin
        // relatif à l'app : le bouton renverrait sur notre propre 404.
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Tarte au sucre",
            Url = "ricardocuisine.com/recettes/123"
        };

        ArrangeAlone(recipe);

        var cut = Render<RecipeDetails>(parameters => parameters.Add(p => p.Id, recipe.Id));

        cut.WaitForAssertion(() =>
            Assert.Contains("href=\"https://ricardocuisine.com/recettes/123\"", cut.Markup));
    }

    [Fact]
    public void HomeRecipe_ShowsHomeBanner_SoTheBannerIsNeverAbsent()
    {
        var recipe = new Recipe { Id = 1, Name = "Sauce à spaghetti de mémé" };

        ArrangeAlone(recipe);

        var cut = Render<RecipeDetails>(parameters => parameters.Add(p => p.Id, recipe.Id));

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("D'où elle vient", cut.Markup);
            Assert.Contains("Recette maison", cut.Markup);
        });
    }

    #endregion

    #region Le panneau latéral

    [Fact]
    public void BookRecipe_ListsItsNeighboursWithTheirPageNumbers()
    {
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Crème brûlée",
            BookId = 7,
            BookPage = 62,
            Book = new Book { Id = 7, Name = "Made in India" }
        };

        StubEmptyNeighbours();
        StubRecipe(recipe);

        var neighbours = new List<Recipe>
        {
            recipe,
            new() { Id = 2, Name = "Dal", BookId = 7, BookPage = 44 },
            new() { Id = 3, Name = "Naan", BookId = 7 }
        };

        _recipeService.SearchAsync(
                Arg.Any<string?>(), Arg.Any<int?>(), 7, Arg.Any<int?>(), Arg.Any<int?>(),
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<bool>(),
                Arg.Any<IReadOnlyCollection<int>?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<(IReadOnlyList<Recipe> Items, int Total)>.Success((neighbours, 3))));

        var cut = Render<RecipeDetails>(parameters => parameters.Add(p => p.Id, 1));

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Aussi dans ce livre", cut.Markup);
            Assert.Contains("p. 44", cut.Markup);
            // La recette affichée ne se liste pas elle-même.
            Assert.DoesNotContain("href=\"/recipes/1\"", cut.Markup);
            // Le compte du bandeau et le lien du panneau lisent le même total.
            Assert.Contains("3 recettes de ce livre", cut.Markup);
            Assert.Contains("Voir les 3 recettes", cut.Markup);
        });
    }

    [Fact]
    public void AloneInItsBook_FallsBackToRecipesSharingATag()
    {
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Crème brûlée",
            BookId = 7,
            BookPage = 62,
            Book = new Book { Id = 7, Name = "Made in India" },
            Etiquettes = [new Etiquette { Id = 9, Name = "Dessert" }]
        };

        StubEmptyNeighbours();
        StubRecipe(recipe);

        // Le repli n'est déclenché que par la recherche par étiquette : la requête du
        // livre reste vide, comme pour un livre à recette unique.
        _recipeService.SearchAsync(
                Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(),
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<bool>(),
                Arg.Is<IReadOnlyCollection<int>?>(ids => ids != null && ids.Contains(9)),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<(IReadOnlyList<Recipe> Items, int Total)>.Success(
                (new List<Recipe> { new() { Id = 42, Name = "Pouding chômeur", Rating = 5 } }, 1))));

        var cut = Render<RecipeDetails>(parameters => parameters.Add(p => p.Id, 1));

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Mêmes étiquettes", cut.Markup);
            Assert.Contains("Pouding chômeur", cut.Markup);
            Assert.Contains("5 pizzas", cut.Markup);
        });
    }

    #endregion

    #region Ce qui disparaît

    [Fact]
    public void DropsTheSingleTabbedPanelAndTheSimilarRecipesGrid()
    {
        var recipe = new Recipe { Id = 1, Name = "Sauce à spaghetti de mémé" };

        ArrangeAlone(recipe);

        var cut = Render<RecipeDetails>(parameters => parameters.Add(p => p.Id, recipe.Id));

        cut.WaitForAssertion(() =>
        {
            // Un jeu d'onglets qui n'a qu'un onglet n'est pas un jeu d'onglets.
            Assert.DoesNotContain("mud-tabs", cut.Markup);
            Assert.DoesNotContain("Recettes Similaires", cut.Markup);
        });
    }

    #endregion
}
