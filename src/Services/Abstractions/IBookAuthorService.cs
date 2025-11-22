using RecettesIndex.Models;

namespace RecettesIndex.Services.Abstractions;

/// <summary>
/// Service for managing the many-to-many relationship between books and authors.
/// </summary>
public interface IBookAuthorService
{
    /// <summary>
    /// Creates associations between a book and multiple authors (for new books).
    /// </summary>
    /// <param name="bookId">The ID of the book to associate authors with.</param>
    /// <param name="authors">The collection of authors to associate with the book.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task CreateBookAuthorAssociationsAsync(int bookId, IEnumerable<Author> authors);

    /// <summary>
    /// Updates associations between a book and authors (for existing books).
    /// Only modifies what has changed for better performance.
    /// </summary>
    /// <param name="bookId">The ID of the book to update associations for.</param>
    /// <param name="newAuthors">The new collection of authors to associate with the book.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task UpdateBookAuthorAssociationsAsync(int bookId, IEnumerable<Author> newAuthors);

    /// <summary>
    /// Loads authors for a specific book using the junction table and populates the book's Authors property.
    /// </summary>
    /// <param name="book">The book to load authors for. Its Authors property will be populated.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task LoadAuthorsForBookAsync(Book book);
}
