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

    [Fact]
    public void RendersNotFoundEmptyState_WhenRecipeDoesNotExist()
    {
        _recipeService.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Recipe>.Failure("Recipe with ID 999 not found")));

        _recipeService.SearchAsync(
                Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(),
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<(IReadOnlyList<Recipe> Items, int Total)>.Success((Array.Empty<Recipe>(), 0))));

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
}
