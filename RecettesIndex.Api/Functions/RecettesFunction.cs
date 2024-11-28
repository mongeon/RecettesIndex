using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using RecettesIndex.Api.Data;

namespace RecettesIndex.Api.Functions;

public class RecettesFunction(ILogger<RecettesFunction> logger, IRecetteRepository recetteRepository)
{
    private readonly ILogger<RecettesFunction> _logger = logger;

    [Function("GetAllRecettes")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "recettes")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var recettes = await recetteRepository.GetRecettes();

        Shared.Recette[] recettesDTO = recettes.Select(r => new Shared.Recette
        {
            Id = r.Id,
            Name = r.Name,
            CreatedAt = r.CreatedAt,
            BookId = r.Book_Id
        }).ToArray();

        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);

        await response.WriteAsJsonAsync(recettesDTO);

        return response;
    }
}
