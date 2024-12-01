using RecettesIndex.Shared;
using System.Text.Json;

namespace RecettesIndex.Data;

public class BookRepository(HttpClient client) : BaseRepository(client), IBookRepository
{
    public async Task<Book[]> GetBooks()
    {
        using var response = await _client.GetAsync("api/books");
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        var books = await JsonSerializer.DeserializeAsync<Book[]>(stream, _options);

        return books ?? [];
    }
}
