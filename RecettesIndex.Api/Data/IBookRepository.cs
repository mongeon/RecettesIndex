using RecettesIndex.Api.Data.Models;

namespace RecettesIndex.Api.Data;

public interface IBookRepository
{
    Task<Book?> GetBook(int id);
    Task<IEnumerable<Book>> GetBooks();
    Task<Book?> Insert(Book book);
}