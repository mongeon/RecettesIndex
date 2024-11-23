using RecettesIndex.Shared;
using System.Net.Http.Json;

namespace RecettesIndex.Data;

public class RecetteRepository(HttpClient client) : BaseRepository(client), IRecetteRepository
{
    public async Task<IEnumerable<Recette>> GetRecettes()
    {
        using var response = await client.GetAsync("api/recettes");
        response.EnsureSuccessStatusCode();

        var recettes = await response.Content.ReadFromJsonAsync<IEnumerable<Recette>>(_options);

        return recettes;
    }
}
