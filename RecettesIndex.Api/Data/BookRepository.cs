using RecettesIndex.Api.Data.Models;

namespace RecettesIndex.Api.Data;

public class BookRepository(Supabase.Client client) : IBookRepository
{
    public async Task<IEnumerable<Book>> GetBooks()
    {
        var result = await client
            .From<Book>()
            .Get();

        return result.Models;
    }

    public async Task<Book?> Insert(Book book)
    {
        var result = await client.From<Book>().Insert(book);

        return result.Model;
    }
}