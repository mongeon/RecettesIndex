namespace RecettesIndex.Api.Data;

public interface IRecetteRepository
{
    Task<IEnumerable<Recette>> GetRecettes();
    Task<Recette?> Insert(Recette recette);
}