using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using RecettesIndex.Api.Data;
using RecettesIndex.Api.Data.Converter;

namespace RecettesIndex.Api.Functions;

public class AuthorsFunction(ILogger<AuthorsFunction> logger, IAuthorRepository kRepository)
{
    private readonly ILogger<authorsFunction> _logger = logger;

    [Function("GetAllAuthors")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "authors")] HttpRequestData req)
    {
        var authors = await authorRepository.GetAuthors();
        Shared.Author[] authorsDTO = authors.Select(r => r.Convert()).ToArray();

        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);

        await response.WriteAsJsonAsync(authorsDTO);

        return response;
    }

    [Function("GetAuthor")]
    public async Task<HttpResponseData> RunGetAuthor([HttpTrigger(AuthorizationLevel.Function, "get", Route = "authors/{id:int}")] HttpRequestData req,
        int id)
    {
        var author = await authorRepository.GetAuthor(id);

        Shared.Author authorDTO = author.Convert();

        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);

        await response.WriteAsJsonAsync(authorDTO);

        return response;
    }

}
