using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RecettesIndex.Api.Data;

namespace RecettesIndex.Api;

public class Function(ILogger<Function> logger, IRecetteRepository recetteRepository, IConfiguration config)
{
    private readonly ILogger<Function> _logger = logger;

    [Function("GetAllRecettes")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "recettes")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var configValue = config.AsEnumerable();
        var recettes = await recetteRepository.GetRecettes();

        Shared.Recette[] recettesDTO = recettes.Select(r => new Shared.Recette
        {
            Id = r.Id,
            Name = r.Name,
            CreatedAt = r.CreatedAt
        }).ToArray();

        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);

        await response.WriteAsJsonAsync(recettesDTO);

        return response;
    }
}
