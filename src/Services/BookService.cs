using Microsoft.Extensions.Logging;
using RecettesIndex.Models;
using RecettesIndex.Services.Abstractions;

namespace RecettesIndex.Services;

/// <summary>
/// Service for managing book operations with caching and error handling.
/// </summary>
public class BookService(
    IBookAuthorService bookAuthorService,
    ICacheService cache,
    Supabase.Client supabaseClient,
    ILogger<BookService> logger) : IBookService
{
    private readonly IBookAuthorService _bookAuthorService = bookAuthorService ?? throw new ArgumentNullException(nameof(bookAuthorService));
    private readonly ICacheService _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly Supabase.Client _supabaseClient = supabaseClient ?? throw new ArgumentNullException(nameof(supabaseClient));
    private readonly ILogger<BookService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheConstants.BooksListKey,
            CacheConstants.DefaultTtl,
            async ct =>
            {
                try
                {
                    var response = await _supabaseClient.From<Book>().Get(cancellationToken: ct);
                    var books = response.Models ?? [];
                    
                    // Load authors for each book
                    foreach (var book in books)
                    {
                        await _bookAuthorService.LoadAuthorsForBookAsync(book);
                    }
                    
                    return (IReadOnlyList<Book>)books;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading all books");
                    return Array.Empty<Book>();
                }
            },
            ct);
    }

    public async Task<Result<Book>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var book = await _supabaseClient.From<Book>()
                .Where(x => x.Id == id)
                .Single();

            if (book == null)
            {
                return Result<Book>.Failure($"Book with ID {id} not found");
            }

            await _bookAuthorService.LoadAuthorsForBookAsync(book);
            
            return Result<Book>.Success(book);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while getting book by id: {BookId}", id);
            return Result<Book>.Failure("Network error. Please check your connection.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting book by id: {BookId}", id);
            return Result<Book>.Failure("An unexpected error occurred while loading the book");
        }
    }

    public async Task<Result<Book>> CreateAsync(Book book, IEnumerable<int> authorIds, CancellationToken ct = default)
    {
        try
        {
            // Validate input
            if (book == null)
            {
                return Result<Book>.Failure("Book cannot be null");
            }
            
            if (string.IsNullOrWhiteSpace(book.Name))
            {
                return Result<Book>.Failure("Book name is required");
            }

            // Set creation date
            book.CreationDate = DateTime.UtcNow;

            // Insert book
            var response = await _supabaseClient.From<Book>().Insert(book);
            var createdBook = response.Models?.FirstOrDefault();

            if (createdBook == null)
            {
                return Result<Book>.Failure("Failed to create book");
            }

            // Create author associations
            var authorIdsList = authorIds.ToList();
            if (authorIdsList.Any())
            {
                // Get author objects to populate the Authors list
                var authorsResponse = await _supabaseClient.From<Author>()
                    .Filter("id", Supabase.Postgrest.Constants.Operator.In, authorIdsList)
                    .Get(cancellationToken: ct);
                
                var authors = authorsResponse.Models ?? [];
                
                await _bookAuthorService.CreateBookAuthorAssociationsAsync(createdBook.Id, authors);
                createdBook.Authors = authors;
            }

            // Invalidate cache
            _cache.Remove(CacheConstants.BooksListKey);

            _logger.LogInformation("Book created successfully: {BookId}", createdBook.Id);
            return Result<Book>.Success(createdBook);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while creating book");
            return Result<Book>.Failure("Network error. Please check your connection.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating book");
            return Result<Book>.Failure("An unexpected error occurred while creating the book");
        }
    }

    public async Task<Result<Book>> UpdateAsync(Book book, IEnumerable<int> authorIds, CancellationToken ct = default)
    {
        try
        {
            // Validate input
            if (book == null)
            {
                return Result<Book>.Failure("Book cannot be null");
            }
            
            if (string.IsNullOrWhiteSpace(book.Name))
            {
                return Result<Book>.Failure("Book name is required");
            }
            
            if (book.Id <= 0)
            {
                return Result<Book>.Failure("Invalid book ID");
            }

            // Update book
            var response = await _supabaseClient.From<Book>()
                .Where(x => x.Id == book.Id)
                .Update(book);
                
            var updatedBook = response.Models?.FirstOrDefault();

            if (updatedBook == null)
            {
                return Result<Book>.Failure("Failed to update book");
            }

            // Update author associations
            var authorIdsList = authorIds.ToList();
            var authorsResponse = await _supabaseClient.From<Author>()
                .Filter("id", Supabase.Postgrest.Constants.Operator.In, authorIdsList)
                .Get(cancellationToken: ct);
            
            var authors = authorsResponse.Models ?? [];
            await _bookAuthorService.UpdateBookAuthorAssociationsAsync(updatedBook.Id, authors);
            updatedBook.Authors = authors;

            // Invalidate cache
            _cache.Remove(CacheConstants.BooksListKey);

            _logger.LogInformation("Book updated successfully: {BookId}", updatedBook.Id);
            return Result<Book>.Success(updatedBook);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while updating book: {BookId}", book.Id);
            return Result<Book>.Failure("Network error. Please check your connection.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating book: {BookId}", book.Id);
            return Result<Book>.Failure("An unexpected error occurred while updating the book");
        }
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        try
        {
            // Validate input
            if (id <= 0)
            {
                return Result<bool>.Failure("Invalid book ID");
            }

            // Check if book exists
            var existingBook = await _supabaseClient.From<Book>()
                .Where(x => x.Id == id)
                .Single();

            if (existingBook == null)
            {
                return Result<bool>.Failure($"Book with ID {id} not found");
            }

            // Delete book (cascade will handle book-author associations)
            await _supabaseClient.From<Book>()
                .Where(x => x.Id == id)
                .Delete();

            // Invalidate cache
            _cache.Remove(CacheConstants.BooksListKey);

            _logger.LogInformation("Book deleted successfully: {BookId}", id);
            return Result<bool>.Success(true);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while deleting book: {BookId}", id);
            return Result<bool>.Failure("Network error. Please check your connection.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting book: {BookId}", id);
            return Result<bool>.Failure("An unexpected error occurred while deleting the book");
        }
    }
}
