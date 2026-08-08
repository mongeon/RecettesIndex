using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using NSubstitute;
using RecettesIndex.Pages;
using RecettesIndex.Services;

namespace RecettesIndex.Tests.Pages;

/// <summary>
/// The books list is now a redirect: Books, Stores and Authors were three near-identical
/// tables and are merged into Sources. What is worth testing is that the old route still
/// leads somewhere useful, since bookmarks and written-down links point at it.
/// </summary>
public class BooksPageTests : BunitContext
{
    public BooksPageTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();

        var authWrapper = Substitute.For<ISupabaseAuthWrapper>();
        Services.AddSingleton(new AuthService(authWrapper));
    }

    [Fact]
    public void RedirectsToSources_KeepingTheBookFilter()
    {
        // Le signet qui voulait dire « les livres » doit continuer de montrer les livres,
        // pas déverser toutes les sources d'un coup.
        Render<Books>();

        var navigation = Services.GetRequiredService<NavigationManager>();
        Assert.EndsWith("/sources?type=book", navigation.Uri);
    }
}
