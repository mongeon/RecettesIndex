using RecettesIndex.Api.Data.Models;

namespace RecettesIndex.Api.Data;

public interface IRecetteRepository
{
    Task<IEnumerable<Recette>> GetRecettes();
    Task<Recette?> GetRecette(int id);
    Task<Recette?> Insert(Recette recette);
    Task<IEnumerable<Recette>> GetRecettesByBook(int bookId);
}