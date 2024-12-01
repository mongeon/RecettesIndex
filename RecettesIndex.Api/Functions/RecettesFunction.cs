using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using RecettesIndex.Api.Data;
using RecettesIndex.Api.Data.Converter;

namespace RecettesIndex.Api.Functions;

public class RecettesFunction(ILogger<RecettesFunction> logger, IRecetteRepository recetteRepository)
{
    private readonly ILogger<RecettesFunction> _logger = logger;

    [Function("GetAllRecettes")]
    public async Task<HttpResponseData> RunGetAllRecettes([HttpTrigger(AuthorizationLevel.Function, "get", Route = "recettes")] HttpRequestData req)
    {
        _logger.LogInformation("Get All Recettes");

        var recettes = await recetteRepository.GetRecettes();

        Shared.Recette[] recettesDTO = recettes
            .Select(r => r.Convert())
            .ToArray();

        _logger.LogInformation("Recettes found: {Count}", recettesDTO.Length);

        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);

        await response.WriteAsJsonAsync(recettesDTO);

        return response;
    }
    [Function("GetRecette")]
    public async Task<HttpResponseData> RunGetRecette([HttpTrigger(AuthorizationLevel.Function, "get", Route = "recettes/{id:int}")] HttpRequestData req,
    int id)
    {
        _logger.LogInformation("Get Recette {Id}", id);
        var recette = await recetteRepository.GetRecette(id);

        Shared.Recette? recetteDTO = recette?.Convert();

        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);

        await response.WriteAsJsonAsync(recetteDTO);

        return response;
    }

    [Function("GetRecettesByBook")]
    public async Task<HttpResponseData> RunGetRecettesByBook([HttpTrigger(AuthorizationLevel.Function, "get", Route = "recettes/book/{bookId:int}")] HttpRequestData req,
        int bookId)
    {
        _logger.LogInformation("Get Recettes by Book {BookId}", bookId);
        var recettes = await recetteRepository.GetRecettesByBook(bookId);
        Shared.Recette[] recettesDTO = recettes
            .Select(r => r.Convert())
            .ToArray();
        _logger.LogInformation("Recettes found: {Count}", recettesDTO.Length);
        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await response.WriteAsJsonAsync(recettesDTO);
        return response;
    }

    [Function("GetRecettesByAuthor")]
    public async Task<HttpResponseData> RunGetRecettesByAuthor([HttpTrigger(AuthorizationLevel.Function, "get", Route = "recettes/author/{authorId:int}")] HttpRequestData req,
        int authorId)
    {
        _logger.LogInformation("Get Recettes by Author {AuthorId}", authorId);
        var recettes = await recetteRepository.GetRecettesByAuthor(authorId);
        Shared.Recette[] recettesDTO = recettes
            .Select(r => r.Convert())
            .ToArray();
        _logger.LogInformation("Recettes found: {Count}", recettesDTO.Length);
        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await response.WriteAsJsonAsync(recettesDTO);
        return response;
    }

    [Function("GetLatestRecettes")]
    public async Task<HttpResponseData> RunGetLatestRecettes([HttpTrigger(AuthorizationLevel.Function, "get", Route = "recettes/latest/{count:int}")] HttpRequestData req,
        int count = 5)
    {
        _logger.LogInformation("Get Latest Recettes");
        var recettes = await recetteRepository.GetRecettes();
        Shared.Recette[] recettesDTO = recettes
            .OrderByDescending(r => r.CreatedAt)
            .Take(count)
            .Select(r => r.Convert())
            .ToArray();
        _logger.LogInformation("Recettes found: {Count}", recettesDTO.Length);
        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await response.WriteAsJsonAsync(recettesDTO);
        return response;
    }
}
