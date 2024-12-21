using RecettesIndex.Api.Data.Models;

namespace RecettesIndex.Api.Data;

public class RecetteRepository(Supabase.Client client) : IRecetteRepository
{
    public async Task<IEnumerable<Recette>> GetRecettes()
    {
        var result = await client
            .From<Recette­­>()
            .Get();

        return result.Models;
    }

    public async Task<Recette?> GetRecette(int id)
    {
        var result = await client
            .From<Recette>()
            .Where(b => b.Id == id)
            .Single();

        return result;
    }

    public async Task<Recette?> Insert(Recette recette)
    {
        var result = await client.From<Recette>().Insert(recette);

        return result.Model;
    }

    public async Task<IEnumerable<Recette>> GetRecettesByBook(int bookId)
    {
        var result = await client
            .From<Recette>()
            .Where(b => b.BookId == bookId)
            .Get();
        return result.Models;
    }

    public async Task<IEnumerable<Recette>> GetRecettesByAuthor(int authorId)
    {
        //var result = await client
        //    .From<Recette>()
        //    .Select("*, book:book_id(*, author(*))")
        //    .Filter("book.author.id", Supabase.Postgrest.Constants.Operator.Equals, authorId)
        //    .Get();
        //return result.Models.Where(x => x.Book?.Authors.Any(a => a.Id == authorId) ?? false);
        return new List<Recette>();
    }
}
//txJdjTAIB1qBemkK