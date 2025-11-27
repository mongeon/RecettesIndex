using Microsoft.Extensions.Logging;
using RecettesIndex.Models;
using RecettesIndex.Services.Abstractions;

namespace RecettesIndex.Services;

// <summary>
// Service for managing book operations with caching and error handling.
// </summary>
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
        return await _cache.GetOrEmptyAsync(
            CacheConstants.BooksListKey,
            CacheConstants.DefaultTtl,
            async token =>
            {
                var response = await _supabaseClient.From<Book>().Get(cancellationToken: token);
                var books = response.Models ?? [];
                
                foreach (var book in books)
                {
                    await _bookAuthorService.LoadAuthorsForBookAsync(book);
                }
                
                return (IReadOnlyList<Book>)books;
            },
            _logger,
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
            var err = ValidationGuards.RequireNotNull(book, "Book");
            if (err != null) return Result<Book>.Failure(err);
            
            err = ValidationGuards.RequireNonEmpty(book.Name, "Book name");
            if (err != null) return Result<Book>.Failure(err);

            book.CreationDate = DateTime.UtcNow;

            var response = await _supabaseClient.From<Book>().Insert(book);
            var createdBook = response.Models?.FirstOrDefault();

            if (createdBook == null)
            {
                return Result<Book>.Failure("Failed to create book");
            }

            var authorIdsList = authorIds.ToList();
            if (authorIdsList.Any())
            {
                var authorsResponse = await _supabaseClient.From<Author>()
                    .Filter("id", Supabase.Postgrest.Constants.Operator.In, authorIdsList)
                    .Get(cancellationToken: ct);
                
                var authors = authorsResponse.Models ?? [];
                
                await _bookAuthorService.CreateBookAuthorAssociationsAsync(createdBook.Id, authors);
                createdBook.Authors = authors;
            }

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
            var err = ValidationGuards.RequireNotNull(book, "Book");
            if (err != null) return Result<Book>.Failure(err);
            
            err = ValidationGuards.RequireNonEmpty(book.Name, "Book name");
            if (err != null) return Result<Book>.Failure(err);
            
            err = ValidationGuards.RequirePositive(book.Id, "book ID");
            if (err != null) return Result<Book>.Failure(err);

            var response = await _supabaseClient.From<Book>()
                .Where(x => x.Id == book.Id)
                .Update(book);
                
            var updatedBook = response.Models?.FirstOrDefault();

            if (updatedBook == null)
            {
                return Result<Book>.Failure("Failed to update book");
            }

            var authorIdsList = authorIds.ToList();
            var authorsResponse = await _supabaseClient.From<Author>()
                .Filter("id", Supabase.Postgrest.Constants.Operator.In, authorIdsList)
                .Get(cancellationToken: ct);
            
            var authors = authorsResponse.Models ?? [];
            await _bookAuthorService.UpdateBookAuthorAssociationsAsync(updatedBook.Id, authors);
            updatedBook.Authors = authors;

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
            var err = ValidationGuards.RequirePositive(id, "book ID");
            if (err != null) return Result<bool>.Failure(err);

            var existingBook = await _supabaseClient.From<Book>()
                .Where(x => x.Id == id)
                .Single();

            if (existingBook == null)
            {
                return Result<bool>.Failure($"Book with ID {id} not found");
            }

            await _supabaseClient.From<Book>()
                .Where(x => x.Id == id)
                .Delete();

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
