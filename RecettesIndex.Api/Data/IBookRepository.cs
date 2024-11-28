using RecettesIndex.Api.Data.Models;

namespace RecettesIndex.Api.Data;

public interface IBookRepository
{
    Task<IEnumerable<Book>> GetBooks();
    Task<Book?> Insert(Book book);
}