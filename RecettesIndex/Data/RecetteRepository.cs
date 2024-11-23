using RecettesIndex.Shared;
using System.Text.Json;

namespace RecettesIndex.Data;

public class RecetteRepository(HttpClient client) : BaseRepository(client), IRecetteRepository
{
    public async Task<IEnumerable<Recette>> GetRecettes()
    {
        using var response = await client.GetAsync("api/recettes");
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        var recettes = await JsonSerializer.DeserializeAsync<IEnumerable<Recette>>(stream, _options);

        return recettes;
    }
}
