using RecettesIndex.Models;

namespace RecettesIndex.Services.Abstractions;

/// <summary>
/// Service for managing recipe tags and their attachment to recipes.
/// </summary>
public interface IEtiquetteService
{
    /// <summary>
    /// Retrieves all tags, ordered by name.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of all tags.</returns>
    Task<IReadOnlyList<Etiquette>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Retrieves an existing tag by name, or creates it when no match exists.
    /// </summary>
    /// <param name="name">The tag label; compared case-insensitively and trimmed.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing the existing or newly created tag.</returns>
    /// <remarks>
    /// Backs the "create on the fly" behaviour of the tag picker: typing a tag that does
    /// not exist yet offers to create it without leaving the dialog.
    /// </remarks>
    Task<Result<Etiquette>> GetOrCreateAsync(string name, CancellationToken ct = default);

    /// <summary>
    /// Replaces the full set of tags attached to a recipe.
    /// </summary>
    /// <param name="recipeId">The recipe to retag.</param>
    /// <param name="etiquetteIds">The tag IDs the recipe should end up with.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    /// <remarks>
    /// Computes the difference against what is already stored, so an unchanged set costs
    /// no writes and re-tagging never drops and recreates rows it could have kept.
    /// </remarks>
    Task<Result<bool>> SetForRecipeAsync(int recipeId, IReadOnlyCollection<int> etiquetteIds, CancellationToken ct = default);

    /// <summary>
    /// Deletes a tag and, by cascade, every attachment of it to a recipe.
    /// </summary>
    /// <param name="id">The ID of the tag to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default);
}
