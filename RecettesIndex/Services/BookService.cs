using RecettesIndex.Api.Data.Converter;
using RecettesIndex.Data;
using RecettesIndex.Shared;

namespace RecettesIndex.Services;

public class BookService(ILogger<BookService> logger, IBookRepository bookRepository)
{
    private readonly ILogger<BookService> _logger = logger;

    public async Task<Book?> GetBook(int id)
    {
        _logger.LogInformation("Get Book {Id}", id);
        var book = await bookRepository.GetBook(id);
        Shared.Book? bookDTO = book?.Convert();
        return bookDTO;
    }
    public async Task<Book[]> GetBooks()
    {
        _logger.LogInformation("Get All Books");
        var books = await bookRepository.GetBooks();
        Shared.Book[] booksDTO = books.Select(r => r.Convert()!).ToArray();
        _logger.LogInformation("Books found: {Count}", booksDTO.Length);

        return booksDTO;
    }
    public async Task<Author?> Insert(Author author)
    {
        throw new NotImplementedException();
    }
    public async Task<Book[]> GetBooksByAuthor(int authorId)
    {
        _logger.LogInformation("Get Books by Author {AuthorId}", authorId);
        var books = await bookRepository.GetBooksByAuthor(authorId);
        Shared.Book[] booksDTO = books.Select(r => r.Convert()!).ToArray();
        _logger.LogInformation("Books found: {Count}", booksDTO.Length);
        return booksDTO;
    }
}
