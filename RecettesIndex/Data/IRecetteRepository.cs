using RecettesIndex.Shared;

namespace RecettesIndex.Data
{
    public interface IRecetteRepository
    {
        Task<IEnumerable<Recette>> GetRecettes();
        // Task<Recette?> Insert(Recette recette);
    }
}
