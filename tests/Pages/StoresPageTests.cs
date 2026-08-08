using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using NSubstitute;
using RecettesIndex.Pages;
using RecettesIndex.Services;

namespace RecettesIndex.Tests.Pages;

/// <summary>
/// The shops list is now a redirect into Sources, like the books and the authors.
/// </summary>
public class StoresPageTests : BunitContext
{
    public StoresPageTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();

        var authWrapper = Substitute.For<ISupabaseAuthWrapper>();
        Services.AddSingleton(new AuthService(authWrapper));
    }

    [Fact]
    public void RedirectsToSources_KeepingTheStoreFilter()
    {
        Render<Stores>();

        var navigation = Services.GetRequiredService<NavigationManager>();
        Assert.EndsWith("/sources?type=store", navigation.Uri);
    }
}
