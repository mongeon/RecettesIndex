using System.Text.Json;

namespace RecettesIndex.Data;

public abstract class BaseRepository
{
    internal readonly HttpClient _client;
    internal readonly JsonSerializerOptions _options;

    public BaseRepository(HttpClient client)
    {

        _client = client;
        _options = new JsonSerializerOptions
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
            PropertyNameCaseInsensitive = true
        };
    }
}
