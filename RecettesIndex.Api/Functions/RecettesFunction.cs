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
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "recettes")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var recettes = await recetteRepository.GetRecettes();

        Shared.Recette[] recettesDTO = recettes
            .Select(r => r.Convert())
            .ToArray();

        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);

        await response.WriteAsJsonAsync(recettesDTO);

        return response;
    }
        [Function("GetRecette")]
    public async Task<HttpResponseData> RunGetRecette([HttpTrigger(AuthorizationLevel.Function, "get", Route = "recettes/{id:int}")] HttpRequestData req,
        int id)
    {
        var recette = await recetteRepository.GetRecette(id);

        Shared.Recette recetteDTO = recette.Convert();

        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);

        await response.WriteAsJsonAsync(recetteDTO);

        return response;
    }
}
