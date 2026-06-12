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
    ILogger<BookService> logger) : CrudServiceBase<Book, BookService>(cache, supabaseClient, logger), IBookService
{
    private readonly IBookAuthorService _bookAuthorService = bookAuthorService ?? throw new ArgumentNullException(nameof(bookAuthorService));

    public async Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken ct = default)
    {
        return await GetAllCachedAsync(
            CacheConstants.BooksListKey,
            async token =>
            {
                var response = await _supabaseClient.From<Book>().Get(cancellationToken: token);
                var books = response.Models ?? [];

                await _bookAuthorService.LoadAuthorsForBooksAsync(books);

                return (IReadOnlyList<Book>)books;
            },
            ct);
    }

    public Task<Result<Book>> GetByIdAsync(int id, CancellationToken ct = default)
        => GetByIdCoreAsync(
            id,
            async token => await _supabaseClient.From<Book>().Where(x => x.Id == id).Single(cancellationToken: token),
            $"Book with ID {id} not found",
            "getting book by id",
            "An unexpected error occurred while loading the book",
            ct);

    public Task<Result<Book>> CreateAsync(Book book, IEnumerable<int> authorIds, CancellationToken ct = default)
        => CreateCoreAsync(
            book,
            () =>
            {
                var err = ValidationGuards.RequireNotNull(book, "Book");
                if (err != null) return err;
                err = ValidationGuards.RequireNonEmpty(book.Name, "Book name");
                if (err != null) return err;
                return null;
            },
            async token =>
            {
                book.CreationDate = DateTime.UtcNow;
                var response = await _supabaseClient.From<Book>().Insert(book, cancellationToken: token);
                var createdBook = response.Models?.FirstOrDefault();
                if (createdBook == null) return null;

                var authorIdsList = authorIds.ToList();
                if (authorIdsList.Any())
                {
                    var authorsResponse = await _supabaseClient.From<Author>()
                        .Filter("id", Supabase.Postgrest.Constants.Operator.In, authorIdsList)
                        .Get(cancellationToken: token);
                    var authors = authorsResponse.Models ?? [];
                    await _bookAuthorService.CreateBookAuthorAssociationsAsync(createdBook.Id, authors);
                    createdBook.Authors = authors;
                }
                return createdBook;
            },
            onSuccess: created =>
            {
                _cache.Remove(CacheConstants.BooksListKey);
                _logger.LogInformation("Book created successfully: {BookId}", created.Id);
            },
            unexpectedUserMessage: "An unexpected error occurred while creating the book",
            ct: ct);

    public Task<Result<Book>> UpdateAsync(Book book, IEnumerable<int> authorIds, CancellationToken ct = default)
        => UpdateCoreAsync(
            book,
            () =>
            {
                var err = ValidationGuards.RequireNotNull(book, "Book");
                if (err != null) return err;
                err = ValidationGuards.RequireNonEmpty(book.Name, "Book name");
                if (err != null) return err;
                err = ValidationGuards.RequirePositive(book.Id, "book ID");
                if (err != null) return err;
                return null;
            },
            async token =>
            {
                var response = await _supabaseClient.From<Book>()
                    .Where(x => x.Id == book.Id)
                    .Update(book, cancellationToken: token);
                var updatedBook = response.Models?.FirstOrDefault();
                if (updatedBook == null) return null;

                var authorIdsList = authorIds.ToList();
                var authorsResponse = await _supabaseClient.From<Author>()
                    .Filter("id", Supabase.Postgrest.Constants.Operator.In, authorIdsList)
                    .Get(cancellationToken: token);
                var authors = authorsResponse.Models ?? [];
                await _bookAuthorService.UpdateBookAuthorAssociationsAsync(updatedBook.Id, authors);
                updatedBook.Authors = authors;

                return updatedBook;
            },
            onSuccess: updated =>
            {
                _cache.Remove(CacheConstants.BooksListKey);
                _logger.LogInformation("Book updated successfully: {BookId}", updated.Id);
            },
            unexpectedUserMessage: "An unexpected error occurred while updating the book",
            idForLogging: book?.Id,
            entityNameForLogging: "book",
            ct: ct);

    public Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
        => DeleteCoreAsync(
            id,
            async token => await _supabaseClient.From<Book>().Where(x => x.Id == id).Single(cancellationToken: token),
            async token => await _supabaseClient.From<Book>().Where(x => x.Id == id).Delete(cancellationToken: token),
            onSuccess: () =>
            {
                _cache.Remove(CacheConstants.BooksListKey);
                _logger.LogInformation("Book deleted successfully: {BookId}", id);
            },
            notFoundMessage: $"Book with ID {id} not found",
            unexpectedUserMessage: "An unexpected error occurred while deleting the book",
            entityNameForLogging: "book",
            ct: ct);
}
