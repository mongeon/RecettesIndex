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

    /// <summary>
    /// Loads authors for multiple books in two queries instead of one pair per book
    /// </summary>
    public async Task LoadAuthorsForBooksAsync(IReadOnlyCollection<Book> books)
    {
        if (books.Count == 0)
        {
            return;
        }

        try
        {
            var bookIds = books.Select(b => b.Id).ToList();

            var bookAuthorsResponse = await _supabaseClient.From<BookAuthor>()
                .Filter("book_id", Supabase.Postgrest.Constants.Operator.In, bookIds)
                .Get();
            var associations = bookAuthorsResponse.Models ?? [];

            var authorIds = associations.Select(ba => ba.AuthorId).Distinct().ToList();
            var authorsById = new Dictionary<int, Author>();
            if (authorIds.Count > 0)
            {
                var authorsResponse = await _supabaseClient.From<Author>()
                    .Filter("id", Supabase.Postgrest.Constants.Operator.In, authorIds)
                    .Get();
                authorsById = (authorsResponse.Models ?? []).ToDictionary(a => a.Id);
            }

            var authorIdsByBookId = associations
                .GroupBy(ba => ba.BookId)
                .ToDictionary(g => g.Key, g => g.Select(ba => ba.AuthorId).ToList());

            foreach (var book in books)
            {
                book.Authors = authorIdsByBookId.TryGetValue(book.Id, out var ids)
                    ? ids.Where(authorsById.ContainsKey).Select(id => authorsById[id]).ToList()
                    : [];
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error loading authors for {BookCount} books", books.Count);
            foreach (var book in books)
            {
                book.Authors = [];
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error loading authors for {BookCount} books", books.Count);
            foreach (var book in books)
            {
                book.Authors = [];
            }
        }
    }
}
