namespace RecettesIndex.Client.Data;

public class RecetteRepository(Supabase.Client client) : IRecetteRepository
{
    public async Task<IEnumerable<Recette>> GetRecettes()
    {
        var result = await client.From<Recette­­>().Get();

        return result.Models;
    }

    public async Task<Recette?> Insert(Recette recette)
    {
        var result = await client.From<Recette>().Insert(recette);

        return result.Model;
    }
}
//txJdjTAIB1qBemkK