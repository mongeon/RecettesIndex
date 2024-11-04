namespace RecettesIndex.Client.Data;

public interface IRecetteRepository
{
    Task<IEnumerable<Recette>> GetRecettes();
    Task<Recette?> Insert(Recette recette);
}