using RecettesIndex.Components;
using RecettesIndex.Models;
using RecettesIndex.Services;
using Xunit;

namespace RecettesIndex.Tests.Components;

/// <summary>
/// Unit tests for RecipeFormModel — the rules the add and the edit dialog now share.
/// </summary>
public class RecipeFormModelTests
{
    private static RecipeFormModel Named(string name = "Crème brûlée") => new() { Name = name };

    #region Validate

    [Fact]
    public void Validate_BlankName_Refuses()
    {
        var model = new RecipeFormModel { Name = "   " };

        Assert.Equal("Donnez un nom à la recette.", model.Validate());
    }

    [Fact]
    public void Validate_NameAlone_IsEnough()
    {
        // La règle du handoff : on peut enregistrer nu et compléter plus tard.
        Assert.Null(Named().Validate());
    }

    [Fact]
    public void Validate_BookSourceWithoutBook_Refuses()
    {
        var model = Named();
        model.Source = RecipeSourceKind.Book;

        Assert.NotNull(model.Validate());
    }

    [Fact]
    public void Validate_StoreSourceWithoutStore_Refuses()
    {
        var model = Named();
        model.Source = RecipeSourceKind.Store;

        Assert.NotNull(model.Validate());
    }

    [Fact]
    public void Validate_WebSourceWithSchemelessUrl_Accepts()
    {
        // C'est ce que produit un copier-coller depuis la barre d'adresse ; le refuser
        // ferait échouer le cas le plus courant.
        var model = Named();
        model.Source = RecipeSourceKind.Web;
        model.Url = "ricardocuisine.com/recettes/123";

        Assert.Null(model.Validate());
    }

    [Fact]
    public void Validate_WebSourceWithNonsense_Refuses()
    {
        var model = Named();
        model.Source = RecipeSourceKind.Web;
        model.Url = "pas une adresse";

        Assert.NotNull(model.Validate());
    }

    #endregion

    #region ApplyTo

    [Fact]
    public void ApplyTo_BookSource_WritesBookAndPage()
    {
        var model = Named();
        model.Source = RecipeSourceKind.Book;
        model.Book = new Book { Id = 7, Name = "Made in India" };
        model.BookPage = 62;

        var recipe = new Recipe();
        model.ApplyTo(recipe);

        Assert.Equal(7, recipe.BookId);
        Assert.Equal(62, recipe.BookPage);
        Assert.Null(recipe.StoreId);
        Assert.Null(recipe.Url);
    }

    [Fact]
    public void ApplyTo_MovingFromBookToStore_LeavesNoStalePageNumber()
    {
        // La faille que la remise à zéro systématique ferme : une recette déplacée d'un
        // livre vers un commerce gardait son numéro de page.
        var recipe = new Recipe { BookId = 7, BookPage = 62 };

        var model = Named();
        model.Source = RecipeSourceKind.Store;
        model.Store = new Store { Id = 3, Name = "Le Marché" };
        model.ApplyTo(recipe);

        Assert.Equal(3, recipe.StoreId);
        Assert.Null(recipe.BookId);
        Assert.Null(recipe.BookPage);
    }

    [Fact]
    public void ApplyTo_HomeSource_ClearsEverySource()
    {
        var recipe = new Recipe { BookId = 7, BookPage = 62, StoreId = 3, Url = "https://example.com" };

        var model = Named();
        model.Source = RecipeSourceKind.Home;
        model.ApplyTo(recipe);

        Assert.Null(recipe.BookId);
        Assert.Null(recipe.BookPage);
        Assert.Null(recipe.StoreId);
        Assert.Null(recipe.Url);
    }

    [Fact]
    public void ApplyTo_WebSource_StoresTheAbsoluteUrl()
    {
        var model = Named();
        model.Source = RecipeSourceKind.Web;
        model.Url = "ricardocuisine.com/recettes/123";

        var recipe = new Recipe();
        model.ApplyTo(recipe);

        Assert.Equal("https://ricardocuisine.com/recettes/123", recipe.Url);
    }

    [Fact]
    public void ApplyTo_LeavesTheCreationDateAlone()
    {
        // La date d'ajout appartient à la recette, pas au formulaire : la boîte de
        // modification la reporte sur l'objet qu'elle construit, et ApplyTo ne doit pas
        // l'écraser en repassant derrière.
        var added = new DateTime(2024, 2, 14, 16, 20, 0, DateTimeKind.Utc);
        var recipe = new Recipe { Id = 1, CreationDate = added };

        Named().ApplyTo(recipe);

        Assert.Equal(added, recipe.CreationDate);
    }

    [Fact]
    public void ApplyTo_TrimsNameAndEmptiesBlankNotes()
    {
        var model = new RecipeFormModel { Name = "  Dal  ", Notes = "   " };

        var recipe = new Recipe();
        model.ApplyTo(recipe);

        Assert.Equal("Dal", recipe.Name);
        Assert.Null(recipe.Notes);
    }

    #endregion

    #region From

    [Fact]
    public void From_ResolvesTheBookAgainstTheLoadedList()
    {
        var books = new List<Book> { new() { Id = 7, Name = "Made in India" } };
        var recipe = new Recipe { Id = 1, Name = "Dal", BookId = 7, BookPage = 44 };

        var model = RecipeFormModel.From(recipe, books, []);

        Assert.Equal(RecipeSourceKind.Book, model.Source);
        Assert.Equal(7, model.Book?.Id);
        Assert.Equal(44, model.BookPage);
    }

    [Fact]
    public void From_ReadsTheTagsAlreadyAttached()
    {
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Dal",
            Etiquettes = [new Etiquette { Id = 9, Name = "Souper" }]
        };

        var model = RecipeFormModel.From(recipe, [], []);

        Assert.Single(model.EtiquetteIds);
        Assert.Contains(9, model.EtiquetteIds);
    }

    #endregion

    #region DiffersFrom

    [Fact]
    public void DiffersFrom_UntouchedClone_IsFalse()
    {
        var model = Named();
        model.Source = RecipeSourceKind.Book;
        model.Book = new Book { Id = 7, Name = "Made in India" };
        model.EtiquetteIds = [3, 1];

        Assert.False(model.DiffersFrom(model.Clone()));
    }

    [Fact]
    public void DiffersFrom_TagOrderAlone_IsFalse()
    {
        // Les étiquettes forment un ensemble : les avoir cochées dans un autre ordre
        // n'est pas une modification à confirmer en fermant.
        var model = Named();
        model.EtiquetteIds = [3, 1];

        var other = model.Clone();
        other.EtiquetteIds = [1, 3];

        Assert.False(model.DiffersFrom(other));
    }

    [Theory]
    [InlineData("name")]
    [InlineData("rating")]
    [InlineData("notes")]
    [InlineData("tags")]
    [InlineData("favorite")]
    [InlineData("page")]
    public void DiffersFrom_AnyEditedField_IsTrue(string field)
    {
        var original = Named();
        var edited = original.Clone();

        switch (field)
        {
            case "name": edited.Name = "Dal"; break;
            case "rating": edited.Rating = 4; break;
            case "notes": edited.Notes = "Doubler le beurre."; break;
            case "tags": edited.EtiquetteIds = [1]; break;
            case "favorite": edited.IsFavorite = true; break;
            case "page": edited.BookPage = 62; break;
        }

        Assert.True(edited.DiffersFrom(original));
    }

    #endregion

    #region ResetForNext

    [Fact]
    public void ResetForNext_KeepsTheSourceAndClearsTheRest()
    {
        // Tout l'intérêt de « enregistrer et ajouter une autre » : saisir un livre entier
        // d'un coup sans rechoisir le livre à chaque recette.
        var model = new RecipeFormModel
        {
            Name = "Dal",
            Source = RecipeSourceKind.Book,
            Book = new Book { Id = 7, Name = "Made in India" },
            BookPage = 44,
            Rating = 5,
            Notes = "Doubler le beurre.",
            IsFavorite = true,
            EtiquetteIds = [1, 2]
        };

        model.ResetForNext();

        Assert.Equal(RecipeSourceKind.Book, model.Source);
        Assert.Equal(7, model.Book?.Id);
        Assert.Equal(string.Empty, model.Name);
        Assert.Null(model.BookPage);
        Assert.Equal(0, model.Rating);
        Assert.Null(model.Notes);
        Assert.False(model.IsFavorite);
        Assert.Empty(model.EtiquetteIds);
    }

    #endregion
}
