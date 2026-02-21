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

public class BooksPageTests : BunitContext
{
    private readonly IBookService _bookService = Substitute.For<IBookService>();

    public BooksPageTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddSingleton(_bookService);
        Services.AddSingleton(Substitute.For<ILogger<Books>>());

        var authWrapper = Substitute.For<ISupabaseAuthWrapper>();
        Services.AddSingleton(new AuthService(authWrapper));
    }

    [Fact]
    public void RendersEmptyState_WhenNoBooksAreReturned()
    {
        _bookService.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<Book>>(Array.Empty<Book>()));

        var cut = Render<Books>();

        cut.WaitForAssertion(() =>
            Assert.Contains("Aucun livre trouvé", cut.Markup));
    }

    [Fact]
    public void RendersErrorState_WhenLoadingBooksFails()
    {
        _bookService.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromException<IReadOnlyList<Book>>(new Exception("Load failure")));

        var cut = Render<Books>();

        cut.WaitForAssertion(() =>
            Assert.Contains("Impossible de charger les livres pour le moment", cut.Markup));
    }
}
