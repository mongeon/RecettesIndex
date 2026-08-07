using RecettesIndex.Models;
using RecettesIndex.Services;
using Xunit;

namespace RecettesIndex.Tests.Services;

/// <summary>
/// Unit tests for RecipeSource, the branching behind the detail page's source banner.
/// </summary>
public class RecipeSourceTests
{
    #region Classify

    [Fact]
    public void Classify_WithBookId_ReturnsBook()
    {
        var recipe = new Recipe { Id = 1, Name = "Crème brûlée", BookId = 7, BookPage = 62 };

        Assert.Equal(RecipeSourceKind.Book, RecipeSource.Classify(recipe));
    }

    [Fact]
    public void Classify_WithStoreId_ReturnsStore()
    {
        var recipe = new Recipe { Id = 1, Name = "Pâté chinois", StoreId = 3 };

        Assert.Equal(RecipeSourceKind.Store, RecipeSource.Classify(recipe));
    }

    [Fact]
    public void Classify_WithUrlOnly_ReturnsWeb()
    {
        var recipe = new Recipe { Id = 1, Name = "Tarte au sucre", Url = "https://ricardocuisine.com/recettes/123" };

        Assert.Equal(RecipeSourceKind.Web, RecipeSource.Classify(recipe));
    }

    [Fact]
    public void Classify_WithNothing_ReturnsHome()
    {
        var recipe = new Recipe { Id = 1, Name = "Sauce à spaghetti de mémé" };

        Assert.Equal(RecipeSourceKind.Home, RecipeSource.Classify(recipe));
    }

    [Fact]
    public void Classify_WithBothBookAndUrl_PrefersBook()
    {
        // Le livre est la réponse la plus précise à « c'était où ? » : un numéro de page
        // vaut mieux qu'un lien, même quand la recette porte les deux.
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Dal",
            BookId = 7,
            BookPage = 44,
            Url = "https://example.com/dal"
        };

        Assert.Equal(RecipeSourceKind.Book, RecipeSource.Classify(recipe));
    }

    [Fact]
    public void Classify_WithBlankUrl_ReturnsHome()
    {
        var recipe = new Recipe { Id = 1, Name = "Pouding chômeur", Url = "   " };

        Assert.Equal(RecipeSourceKind.Home, RecipeSource.Classify(recipe));
    }

    #endregion

    #region SplitUrl

    [Fact]
    public void SplitUrl_DropsWwwPrefix()
    {
        var (domain, _) = RecipeSource.SplitUrl("https://www.ricardocuisine.com/recettes/123");

        Assert.Equal("ricardocuisine.com", domain);
    }

    [Fact]
    public void SplitUrl_KeepsPathAndQuery()
    {
        var (_, path) = RecipeSource.SplitUrl("https://ricardocuisine.com/recettes/123?p=2");

        Assert.Equal("/recettes/123?p=2", path);
    }

    [Fact]
    public void SplitUrl_RootUrl_ReturnsEmptyPath()
    {
        var (domain, path) = RecipeSource.SplitUrl("https://ricardocuisine.com/");

        Assert.Equal("ricardocuisine.com", domain);
        Assert.Equal(string.Empty, path);
    }

    [Fact]
    public void SplitUrl_WithoutScheme_StillParses()
    {
        var (domain, path) = RecipeSource.SplitUrl("ricardocuisine.com/recettes/123");

        Assert.Equal("ricardocuisine.com", domain);
        Assert.Equal("/recettes/123", path);
    }

    [Fact]
    public void SplitUrl_Unparseable_ReturnsInputAsDomain()
    {
        // Mieux vaut un titre bizarre qu'un bandeau vide à l'endroit de la source.
        var (domain, path) = RecipeSource.SplitUrl("pas une url du tout");

        Assert.Equal("pas une url du tout", domain);
        Assert.Equal(string.Empty, path);
    }

    [Fact]
    public void SplitUrl_Blank_ReturnsEmptyPair()
    {
        var (domain, path) = RecipeSource.SplitUrl("   ");

        Assert.Equal(string.Empty, domain);
        Assert.Equal(string.Empty, path);
    }

    #endregion

    #region ExternalHref

    [Fact]
    public void ExternalHref_SchemelessUrl_GetsAnAbsoluteOne()
    {
        // Sans schéma, le navigateur lirait l'adresse comme un chemin relatif à l'app et
        // atterrirait sur notre propre 404 au lieu du site.
        var href = RecipeSource.ExternalHref("ricardocuisine.com/recettes/123");

        Assert.Equal("https://ricardocuisine.com/recettes/123", href);
    }

    [Fact]
    public void ExternalHref_KeepsAnExistingScheme()
    {
        var href = RecipeSource.ExternalHref("http://ricardocuisine.com/recettes/123");

        Assert.Equal("http://ricardocuisine.com/recettes/123", href);
    }

    [Fact]
    public void ExternalHref_UppercaseScheme_IsRecognizedAsAlreadyHavingOne()
    {
        // Le crible « :// » ne porte aucune lettre, donc la casse du schéma n'entre pas en jeu.
        var href = RecipeSource.ExternalHref("HTTPS://ricardocuisine.com/recettes/123");

        Assert.Equal("https://ricardocuisine.com/recettes/123", href);
    }

    [Fact]
    public void ExternalHref_NonHttpScheme_ReturnsNull()
    {
        Assert.Null(RecipeSource.ExternalHref("javascript://x/%0aalert(1)"));
        Assert.Null(RecipeSource.ExternalHref("ftp://example.com/recette.txt"));
    }

    [Fact]
    public void ExternalHref_Unparseable_ReturnsNull()
    {
        Assert.Null(RecipeSource.ExternalHref("pas une url du tout"));
        Assert.Null(RecipeSource.ExternalHref(null));
    }

    #endregion

    #region Domain

    [Fact]
    public void Domain_IsLowercased_SoTwoRecipesOnTheSameSiteMatch()
    {
        Assert.Equal(
            RecipeSource.Domain("https://Ricardocuisine.com/a"),
            RecipeSource.Domain("https://www.ricardocuisine.com/b"));
    }

    [Fact]
    public void Domain_Blank_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, RecipeSource.Domain(null));
    }

    #endregion
}
