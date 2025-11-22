using RecettesIndex.Models;

namespace RecettesIndex.Services.Abstractions;

/// <summary>
/// Service for managing author operations.
/// </summary>
public interface IAuthorService
{
    /// <summary>
    /// Retrieves all authors.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of all authors.</returns>
    Task<IReadOnlyList<Author>> GetAllAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Retrieves an author by their unique identifier.
    /// </summary>
    /// <param name="id">The author ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing the author if found.</returns>
    Task<Result<Author>> GetByIdAsync(int id, CancellationToken ct = default);
    
    /// <summary>
    /// Creates a new author.
    /// </summary>
    /// <param name="author">The author to create.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing the created author.</returns>
    Task<Result<Author>> CreateAsync(Author author, CancellationToken ct = default);
    
    /// <summary>
    /// Updates an existing author.
    /// </summary>
    /// <param name="author">The author with updated values.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing the updated author.</returns>
    Task<Result<Author>> UpdateAsync(Author author, CancellationToken ct = default);
    
    /// <summary>
    /// Deletes an author from the database.
    /// </summary>
    /// <param name="id">The ID of the author to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default);
}
