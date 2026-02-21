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

public class AuthorsPageTests : BunitContext
{
    private readonly IAuthorService _authorService = Substitute.For<IAuthorService>();

    public AuthorsPageTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddSingleton(_authorService);
        Services.AddSingleton(Substitute.For<ILogger<Authors>>());

        var authWrapper = Substitute.For<ISupabaseAuthWrapper>();
        Services.AddSingleton(new AuthService(authWrapper));
    }

    [Fact]
    public void RendersEmptyState_WhenNoAuthorsAreReturned()
    {
        _authorService.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<Author>>(Array.Empty<Author>()));

        var cut = Render<Authors>();

        cut.WaitForAssertion(() =>
            Assert.Contains("Aucun auteur trouvé", cut.Markup));
    }

    [Fact]
    public void RendersErrorState_WhenLoadingAuthorsFails()
    {
        _authorService.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromException<IReadOnlyList<Author>>(new Exception("Load failure")));

        var cut = Render<Authors>();

        cut.WaitForAssertion(() =>
            Assert.Contains("Impossible de charger les auteurs pour le moment", cut.Markup));
    }
}
