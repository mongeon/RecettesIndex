using RecettesIndex.Api.Data.Models;

namespace RecettesIndex.Api.Data;

public class AuthorRepository(Supabase.Client client) : IAuthorRepository
{
    public async Task<Author?> GetAuthor(int id)
    {
        var result = await client
            .From<Author>()
            .Where(b => b.Id == id)
            .Single();

        return result;
    }

    public async Task<IEnumerable<Author>> GetAuthors()
    {
        var result = await client
            .From<Author>()
            .Get();

        return result.Models;
    }

    public async Task<Author?> Insert(Author author)
    {
        var result = await client.From<Author>().Insert(author);

        return result.Model;
    }
}