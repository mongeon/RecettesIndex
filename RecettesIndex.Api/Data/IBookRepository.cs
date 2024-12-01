using RecettesIndex.Api.Data.Models;

namespace RecettesIndex.Api.Data;

public interface IBookRepository
{
    Task<Book?> GetBook(int id);
    Task<IEnumerable<Book>> GetBooks();
    Task<IEnumerable<Book>> GetBooksByAuthor(int authorId);
    Task<Book?> Insert(Book book);
}