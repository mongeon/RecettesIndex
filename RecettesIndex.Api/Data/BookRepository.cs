using RecettesIndex.Api.Data.Models;

namespace RecettesIndex.Api.Data;

public class BookRepository(Supabase.Client client) : IBookRepository
{
    public async Task<Book?> GetBook(int id)
    {
        var result = await client
            .From<Book>()
            .Where(b => b.Id == id)
            .Single();

        return result;
    }

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

    public async Task<IEnumerable<Book>> GetBooksByAuthor(int authorId)
    {
        //List<IPostgrestQueryFilter> andFilter = new List<IPostgrestQueryFilter>()
        //{
        //    new QueryFilter<Book,List<string>>(x=>x.Title, Operator.Contains, new List<string>() { "-" }),
        //};

        //var result = await client
        //    .From<Book>()
        //    .Where(b => b.Authors.Any())
        //    .Get();

        //var result = await client
        //    .From<Book>()
        //    .And(andFilter)
        //    .Get();

        var result = await client.From<Book>()
            .Get();
        return result.Models;
    }
}