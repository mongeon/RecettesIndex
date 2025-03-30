using RecettesIndex.Api.Data.Converter;
using RecettesIndex.Data;
using RecettesIndex.Shared;

namespace RecettesIndex.Services;

public class AuthorService(ILogger<AuthorService> logger, IAuthorRepository authorRepository)
{
    private readonly ILogger<AuthorService> _logger = logger;
    public async Task<Author?> GetAuthor(int id)
    {

        _logger.LogInformation("Get Author {Id}", id);
        var author = await authorRepository.GetAuthor(id);
        Shared.Author? authorDTO = author?.Convert();

        return authorDTO;
    }
    public async Task<Author[]> GetAuthors()
    {
        _logger.LogInformation("Get All Authors");
        var authors = await authorRepository.GetAuthors();
        Shared.Author[] authorsDTO = authors?.Select(r => r.Convert()!).ToArray() ?? [];

        _logger.LogInformation("Authors found: {Count}", authorsDTO.Length);

        return authorsDTO;
    }
    public async Task<Author?> Insert(Author author)
    {
        throw new NotImplementedException();
    }
}
