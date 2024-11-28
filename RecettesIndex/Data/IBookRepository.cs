using RecettesIndex.Shared;

namespace RecettesIndex.Data;

public interface IBookRepository
{
    Task<Book[]> GetBooks();
    // Task<Book?> Insert(Book book);
}
