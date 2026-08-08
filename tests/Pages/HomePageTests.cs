using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using NSubstitute;
using RecettesIndex.Models;
using RecettesIndex.Pages;
using RecettesIndex.Services;
using RecettesIndex.Services.Abstractions;

namespace RecettesIndex.Tests.Pages;

/// <summary>
/// Unit tests for the lean home page (block 3a).
/// </summary>
public class HomePageTests : BunitContext
{
    private readonly IRecipeService _recipeService = Substitute.For<IRecipeService>();
    private readonly IEtiquetteService _etiquetteService = Substitute.For<IEtiquetteService>();
    private readonly ILocalStorageService _localStorage = Substitute.For<ILocalStorageService>();
    private readonly PaletteLauncher _launcher = new();

    public HomePageTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;

        Services.AddMudServices();
        Services.AddSingleton(_recipeService);
        Services.AddSingleton(_etiquetteService);
        Services.AddSingleton(_localStorage);
        Services.AddSingleton(_launcher);
        Services.AddSingleton(Substitute.For<ILogger<Home>>());
        Services.AddSingleton(new AuthService(Substitute.For<ISupabaseAuthWrapper>()));

        StubEmpty();
    }

    #region Helpers

    private void StubEmpty()
    {
        _recipeService.GetRecipeSummariesAsync(Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IReadOnlyList<Recipe>>.Success(Array.Empty<Recipe>())));

        _recipeService.GetRecipesByIdsAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IReadOnlyList<Recipe>>.Success(Array.Empty<Recipe>())));

        _etiquetteService.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<Etiquette>>([]));

        _etiquetteService.GetRecipeCountsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new Dictionary<int, int>()));
    }

    private void StubSummaries(params Recipe[] summaries)
    {
        _recipeService.GetRecipeSummariesAsync(Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IReadOnlyList<Recipe>>.Success(summaries)));
    }

    private void StubRecipes(params Recipe[] recipes)
    {
        _recipeService.GetRecipesByIdsAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IReadOnlyList<Recipe>>.Success(recipes)));
    }

    #endregion

    [Fact]
    public void DropsTheHeroTheStatsAndTheCarousels()
    {
        // L'ancien accueil vendait l'app à quelqu'un qui l'utilise déjà.
        var cut = Render<Home>();

        cut.WaitForAssertion(() =>
        {
            Assert.DoesNotContain("hero-section", cut.Markup);
            Assert.DoesNotContain("Découvrez de Délicieuses Recettes", cut.Markup);
            Assert.DoesNotContain("Parcourir les Recettes", cut.Markup);
            Assert.Contains("Qu'est-ce que vous cherchez", cut.Markup);
        });
    }

    [Fact]
    public void SearchField_AsksTheLauncherForThePalette()
    {
        // Le champ n'est pas une seconde recherche : c'est un bouton vers la palette,
        // qui sait déjà chercher dans les recettes, les livres et les magasins.
        var opened = false;
        _launcher.Requested += () => { opened = true; return Task.CompletedTask; };

        var cut = Render<Home>();
        cut.Find(".home-search").Click();

        Assert.True(opened);
    }

    [Fact]
    public void UnratedCard_CountsThemAllButListsOnlyFive()
    {
        var summaries = Enumerable.Range(1, 8)
            .Select(i => new Recipe { Id = i, Rating = 0, CreationDate = DateTime.UtcNow.AddDays(-i) })
            .ToArray();
        StubSummaries(summaries);
        StubRecipes(summaries.Select(r => new Recipe { Id = r.Id, Name = $"Recette {r.Id}" }).ToArray());

        var cut = Render<Home>();

        cut.WaitForAssertion(() =>
        {
            // Le compteur porte le total, la liste s'arrête à cinq lignes.
            Assert.Contains("À évaluer · 8", cut.Markup);
            Assert.Equal(5, cut.FindAll(".home-row-rate").Count);
        });
    }

    [Fact]
    public void RatedCollection_SaysSoRatherThanShowingAnEmptyCard()
    {
        StubSummaries(new Recipe { Id = 1, Rating = 5 });
        StubRecipes(new Recipe { Id = 1, Name = "Crème brûlée", Rating = 5 });

        var cut = Render<Home>();

        cut.WaitForAssertion(() => Assert.Contains("Tout est noté.", cut.Markup));
    }

    [Fact]
    public void FrequentTags_AreOrderedByUse()
    {
        _etiquetteService.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<Etiquette>>(
            [
                new Etiquette { Id = 1, Name = "Rare" },
                new Etiquette { Id = 2, Name = "Courante" },
                new Etiquette { Id = 3, Name = "Jamais posée" }
            ]));

        _etiquetteService.GetRecipeCountsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new Dictionary<int, int> { [1] = 2, [2] = 9 }));

        var cut = Render<Home>();

        cut.WaitForAssertion(() =>
        {
            var tags = cut.FindAll(".home-tag");
            Assert.Equal(2, tags.Count);
            // La plus utilisée d'abord ; celle qui n'est posée nulle part n'est pas un
            // raccourci, seulement une ligne de plus à lire.
            Assert.Equal("Courante", tags[0].TextContent.Trim());
            Assert.Equal("Rare", tags[1].TextContent.Trim());
        });
    }
}
