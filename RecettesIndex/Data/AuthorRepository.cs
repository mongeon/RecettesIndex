using RecettesIndex.Shared;
using System.Text.Json;

namespace RecettesIndex.Data;

public class AuthorRepository(HttpClient client) : BaseRepository(client), IAuthorRepository
{
    public async Task<Author[]> GetAuthors()
    {
        using var response = await client.GetAsync("api/authors");
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        var authors = await JsonSerializer.DeserializeAsync<Author[]>(stream, _options);

        return authors;
    }
}
