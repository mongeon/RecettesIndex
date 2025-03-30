using RecettesIndex.Api.Data.Converter;
using RecettesIndex.Data;
using RecettesIndex.Shared;

namespace RecettesIndex.Services;

public class RecetteService(ILogger<RecetteService> logger, IRecetteRepository recetteRepository)
{
    private readonly ILogger<RecetteService> _logger = logger;
    public async Task<Recette[]> GetRecettes()
    {
        var recettes = await recetteRepository.GetRecettes();

        Shared.Recette[] recettesDTO = recettes
            .Select(r => r.Convert())
            .ToArray();

        _logger.LogInformation("Recettes found: {Count}", recettesDTO.Length);

        return recettesDTO;
    }
    public async Task<Recette?> GetRecette(int id)
    {
        _logger.LogInformation("Get Recette {Id}", id);
        var recette = await recetteRepository.GetRecette(id);

        Shared.Recette? recetteDTO = recette?.Convert();

        return recetteDTO;
    }
    public async Task<Recette?> Insert(Recette recette)
    {
        var recetteModel = recette.Convert();
        var result = await recetteRepository.Insert(recetteModel);
        return result.Convert();
    }
    public async Task<Recette[]> GetRecettesByBook(int bookId)
    {
        _logger.LogInformation("Get Recettes by Book {BookId}", bookId);
        var recettes = await recetteRepository.GetRecettesByBook(bookId);
        Shared.Recette[] recettesDTO = recettes
            .Select(r => r.Convert())
            .ToArray();
        _logger.LogInformation("Recettes found: {Count}", recettesDTO.Length);
        return recettesDTO;
    }

    public async Task<Recette[]> GetRecettesByAuthor(int authorId)
    {
        _logger.LogInformation("Get Recettes by Author {AuthorId}", authorId);
        var recettes = await recetteRepository.GetRecettesByAuthor(authorId);
        Shared.Recette[] recettesDTO = recettes
            .Select(r => r.Convert())
            .ToArray();
        _logger.LogInformation("Recettes found: {Count}", recettesDTO.Length);
        return recettesDTO;
    }

    public async Task<Recette[]> GetLatestRecettes(int count = 5)
    {
        _logger.LogInformation("Get Latest Recettes");
        var recettes = await recetteRepository.GetRecettes();
        Shared.Recette[] recettesDTO = recettes
            .OrderByDescending(r => r.CreatedAt)
            .Take(count)
            .Select(r => r.Convert())
            .ToArray();
        _logger.LogInformation("Recettes found: {Count}", recettesDTO.Length);
        return recettesDTO;
    }

    public async Task<Recette[]> GetRandomRecettes(int count = 5)
    {
        _logger.LogInformation("Get Random Recettes");
        var recettes = await recetteRepository.GetRecettes();
        Shared.Recette[] recettesDTO = recettes
            .OrderBy(r => Guid.NewGuid())
            .Take(count)
            .Select(r => r.Convert())
            .ToArray();
        _logger.LogInformation("Recettes found: {Count}", recettesDTO.Length);
        return recettesDTO;
    }
}
