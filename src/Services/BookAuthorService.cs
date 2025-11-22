using Microsoft.Extensions.Logging;
using RecettesIndex.Models;
using RecettesIndex.Services.Abstractions;
using RecettesIndex.Services.Exceptions;
using Supabase;

namespace RecettesIndex.Services;

/// <summary>
/// Service for managing the many-to-many relationship between books and authors.
/// </summary>
public class BookAuthorService(Client supabaseClient, ILogger<BookAuthorService> logger) : IBookAuthorService
{
    private readonly Client _supabaseClient = supabaseClient ?? throw new ArgumentNullException(nameof(supabaseClient));
    private readonly ILogger<BookAuthorService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Creates associations between a book and multiple authors (for new books)
    /// </summary>
    public async Task CreateBookAuthorAssociationsAsync(int bookId, IEnumerable<Author> authors)
    {
        if (!authors.Any())
        {
            return;
        }

        try
        {
            var bookAuthors = authors.Select(author => new BookAuthor
            {
                BookId = bookId,
                AuthorId = author.Id
            }).ToList();

            await _supabaseClient.From<BookAuthor>().Insert(bookAuthors);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error creating book-author associations for book {BookId}", bookId);
            throw new ServiceException("Network error. Please check your connection.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating book-author associations for book {BookId}", bookId);
            throw new ServiceException($"Failed to create book-author associations for book {bookId}", ex);
        }
    }

    /// <summary>
    /// Updates associations between a book and authors (for existing books)
    /// Only modifies what has changed for better performance
    /// </summary>
    public async Task UpdateBookAuthorAssociationsAsync(int bookId, IEnumerable<Author> newAuthors)
    {
        try
        {
            // Get current associations
            var currentAssociationsResponse = await _supabaseClient.From<BookAuthor>()
                .Where(x => x.BookId == bookId)
                .Get();

            var currentAuthorIds = currentAssociationsResponse.Models?.Select(x => x.AuthorId).ToHashSet() ?? [];
            var newAuthorIds = newAuthors.Select(x => x.Id).ToHashSet();

            // Find authors to remove and add
            var authorsToRemove = currentAuthorIds.Except(newAuthorIds).ToList();
            var authorsToAdd = newAuthorIds.Except(currentAuthorIds).ToList();

            // Remove associations that are no longer needed
            foreach (var authorIdToRemove in authorsToRemove)
            {
                await _supabaseClient.From<BookAuthor>()
                    .Where(x => x.BookId == bookId && x.AuthorId == authorIdToRemove)
                    .Delete();
            }

            // Add new associations
            if (authorsToAdd.Any())
            {
                var bookAuthorsToAdd = authorsToAdd.Select(authorId => new BookAuthor
                {
                    BookId = bookId,
                    AuthorId = authorId
                }).ToList();

                await _supabaseClient.From<BookAuthor>().Insert(bookAuthorsToAdd);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error updating book-author associations for book {BookId}", bookId);
            throw new ServiceException("Network error. Please check your connection.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating book-author associations for book {BookId}", bookId);
            throw new ServiceException($"Failed to update book-author associations for book {bookId}", ex);
        }
    }

    /// <summary>
    /// Loads authors for a specific book using the junction table
    /// </summary>
    public async Task LoadAuthorsForBookAsync(Book book)
    {
        try
        {
            // Get book-author associations
            var bookAuthorsResponse = await _supabaseClient.From<BookAuthor>()
                .Where(x => x.BookId == book.Id)
                .Get();

            if (bookAuthorsResponse.Models?.Any() == true)
            {
                var authorIds = bookAuthorsResponse.Models.Select(ba => ba.AuthorId).ToList();

                // Get the actual author objects
                var authorsResponse = await _supabaseClient.From<Author>()
                    .Filter("id", Supabase.Postgrest.Constants.Operator.In, authorIds)
                    .Get();

                book.Authors = authorsResponse.Models ?? [];
            }
            else
            {
                book.Authors = [];
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error loading authors for book {BookId}", book.Id);
            book.Authors = [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error loading authors for book {BookId}", book.Id);
            book.Authors = [];
        }
    }
}
