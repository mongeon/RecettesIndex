using RecettesIndex.Shared;

namespace RecettesIndex.Data
{
    public interface IRecetteRepository
    {
        Task<Recette[]> GetRecettes();
        // Task<Recette?> Insert(Recette recette);
    }
}
