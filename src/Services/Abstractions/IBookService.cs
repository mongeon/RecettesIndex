using RecettesIndex.Models;

namespace RecettesIndex.Services.Abstractions;

/// <summary>
/// Service for managing book operations.
/// </summary>
public interface IBookService
{
    /// <summary>
    /// Retrieves all books with their authors.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of all books.</returns>
    Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Retrieves a book by its unique identifier.
    /// </summary>
    /// <param name="id">The book ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing the book if found.</returns>
    Task<Result<Book>> GetByIdAsync(int id, CancellationToken ct = default);
    
    /// <summary>
    /// Creates a new book with associated authors.
    /// </summary>
    /// <param name="book">The book to create.</param>
    /// <param name="authorIds">IDs of authors to associate with the book.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing the created book.</returns>
    Task<Result<Book>> CreateAsync(Book book, IEnumerable<int> authorIds, CancellationToken ct = default);
    
    /// <summary>
    /// Updates an existing book and its author associations.
    /// </summary>
    /// <param name="book">The book with updated values.</param>
    /// <param name="authorIds">IDs of authors to associate with the book.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing the updated book.</returns>
    Task<Result<Book>> UpdateAsync(Book book, IEnumerable<int> authorIds, CancellationToken ct = default);
    
    /// <summary>
    /// Deletes a book from the database.
    /// </summary>
    /// <param name="id">The ID of the book to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default);
}
