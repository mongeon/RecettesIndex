using RecettesIndex.Models;
using RecettesIndex.Services;

namespace RecettesIndex.Components;

/// <summary>
/// What the recipe form holds while it is being filled in, and the rules that decide
/// whether it can be saved.
/// </summary>
/// <remarks>
/// Pure — no service, no rendering — because the add and the edit dialog have to agree
/// on every one of these rules, and the only way to be sure they do is for both to run
/// the same object. It also reuses <see cref="RecipeSourceKind"/> rather than the four
/// magic strings the two dialogs each carried their own copy of: the form now speaks
/// the same vocabulary as the detail page's source banner.
/// </remarks>
public sealed class RecipeFormModel
{
    /// <summary>The four buttons of the segmented selector, in the order they are shown.</summary>
    /// <remarks>
    /// Book first because it is by far the common case, and « maison » second because it
    /// is the one that needs no further input at all.
    /// </remarks>
    public static readonly RecipeSourceKind[] SourceOrder =
    [
        RecipeSourceKind.Book,
        RecipeSourceKind.Home,
        RecipeSourceKind.Store,
        RecipeSourceKind.Web
    ];

    /// <summary>The only required field.</summary>
    public string Name { get; set; } = string.Empty;

    public RecipeSourceKind Source { get; set; } = RecipeSourceKind.Home;

    public Book? Book { get; set; }

    public int? BookPage { get; set; }

    public Store? Store { get; set; }

    public string? Url { get; set; }

    public int Rating { get; set; }

    public string? Notes { get; set; }

    public HashSet<int> EtiquetteIds { get; set; } = [];

    /// <summary>
    /// Favourites live in the browser, not in the database, so the form only records the
    /// intent; the dialog applies it once the recipe has an ID to key it by.
    /// </summary>
    public bool IsFavorite { get; set; }

    /// <summary>
    /// Reads an existing recipe into the form, resolving its book and store against the
    /// lists the dialog already loaded.
    /// </summary>
    public static RecipeFormModel From(
        Recipe recipe,
        IReadOnlyList<Book> books,
        IReadOnlyList<Store> stores,
        bool isFavorite = false)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        return new RecipeFormModel
        {
            Name = recipe.Name,
            Source = RecipeSource.Classify(recipe),
            Book = recipe.Book ?? books.FirstOrDefault(b => b.Id == recipe.BookId),
            BookPage = recipe.BookPage,
            Store = recipe.Store ?? stores.FirstOrDefault(s => s.Id == recipe.StoreId),
            Url = recipe.Url,
            Rating = recipe.Rating,
            Notes = recipe.Notes,
            EtiquetteIds = recipe.Etiquettes.Select(e => e.Id).ToHashSet(),
            IsFavorite = isFavorite
        };
    }

    /// <summary>
    /// Returns the message to show, or null when the form can be saved.
    /// </summary>
    /// <remarks>
    /// Only the name is truly required; everything else is required <em>by the source the
    /// user picked</em>, which is why the checks hang off <see cref="Source"/>. A recipe
    /// can be saved bare and completed later.
    /// </remarks>
    public string? Validate() => this switch
    {
        _ when string.IsNullOrWhiteSpace(Name) => "Donnez un nom à la recette.",
        { Source: RecipeSourceKind.Book, Book: null } => "Choisissez le livre, ou créez-le depuis le champ.",
        { Source: RecipeSourceKind.Store, Store: null } => "Choisissez le commerce, ou créez-le depuis le champ.",
        { Source: RecipeSourceKind.Web } when RecipeSource.ExternalHref(Url) is null =>
            "L'adresse ne mène nulle part — attendu quelque chose comme ricardocuisine.com/…",
        _ => null
    };

    /// <summary>
    /// Writes the form onto a recipe, ready to be sent to the service.
    /// </summary>
    /// <remarks>
    /// Every source column is cleared first and only the chosen one is written back. The
    /// alternative — one branch per source, each remembering to null out the other three —
    /// is where a recipe moved from a book to a shop keeps a stale page number.
    /// The URL is stored in its absolute form so the detail page, the link and the
    /// « same site » grouping all read the same string.
    /// </remarks>
    public void ApplyTo(Recipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        recipe.Name = Name.Trim();
        recipe.Rating = Rating;
        recipe.Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim();

        recipe.BookId = null;
        recipe.BookPage = null;
        recipe.StoreId = null;
        recipe.Url = null;

        switch (Source)
        {
            case RecipeSourceKind.Book:
                recipe.BookId = Book?.Id;
                recipe.BookPage = BookPage;
                break;
            case RecipeSourceKind.Store:
                recipe.StoreId = Store?.Id;
                break;
            case RecipeSourceKind.Web:
                recipe.Url = RecipeSource.ExternalHref(Url) ?? Url?.Trim();
                break;
        }
    }

    /// <summary>
    /// Clears what belongs to one recipe and keeps the source, for « enregistrer et
    /// ajouter une autre » — the point of that button is entering a whole book in one go.
    /// </summary>
    public void ResetForNext()
    {
        Name = string.Empty;
        BookPage = null;
        Rating = 0;
        Notes = null;
        IsFavorite = false;
        EtiquetteIds = [];
    }

    public RecipeFormModel Clone() => new()
    {
        Name = Name,
        Source = Source,
        Book = Book,
        BookPage = BookPage,
        Store = Store,
        Url = Url,
        Rating = Rating,
        Notes = Notes,
        EtiquetteIds = [.. EtiquetteIds],
        IsFavorite = IsFavorite
    };

    /// <summary>
    /// Whether anything has been touched since <paramref name="original"/> — what decides
    /// if closing the dialog needs to ask first.
    /// </summary>
    public bool DiffersFrom(RecipeFormModel original)
    {
        ArgumentNullException.ThrowIfNull(original);
        return Signature() != original.Signature();
    }

    /// <summary>
    /// Flattens the form to one comparable string. Comparing field by field is the same
    /// list written twice, and the second copy is the one that forgets the new field.
    /// </summary>
    private string Signature() => string.Join(
        "\u001f",
        Name.Trim(),
        Source,
        Book?.Id,
        BookPage,
        Store?.Id,
        Url?.Trim(),
        Rating,
        Notes?.Trim(),
        string.Join(',', EtiquetteIds.OrderBy(id => id)),
        IsFavorite);
}
