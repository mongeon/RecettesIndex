using RecettesIndex.Models;

namespace RecettesIndex.Services.Abstractions;

public interface IRecipesQuery
{
    public Task<List<int>> GetRecipeIdsByNameAsync(string term, int? rating, CancellationToken ct = default);
    public Task<List<int>> GetRecipeIdsByBookIdsAsync(IReadOnlyCollection<int> bookIds, int? rating, CancellationToken ct = default);
    public Task<List<int>> GetRecipeIdsByStoreIdsAsync(IReadOnlyCollection<int> storeIds, int? rating, CancellationToken ct = default);
    public Task<List<int>> GetAllRecipeIdsAsync(int? rating, CancellationToken ct = default);
    public Task<IReadOnlyList<Recipe>> GetRecipesByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);

    public Task<List<int>> GetBookIdsByTitleAsync(string term, CancellationToken ct = default);
    public Task<List<int>> GetBookIdsByAuthorIdsAsync(IReadOnlyCollection<int> authorIds, CancellationToken ct = default);
    public Task<List<int>> GetBookIdsByAuthorAsync(int authorId, CancellationToken ct = default);
    public Task<List<int>> GetAuthorIdsByNameAsync(string term, CancellationToken ct = default);
    
    public Task<List<int>> GetStoreIdsByNameAsync(string term, CancellationToken ct = default);

    public Task<IReadOnlyList<Book>> GetBooksAsync(CancellationToken ct = default);
    public Task<IReadOnlyList<Author>> GetAuthorsAsync(CancellationToken ct = default);
    public Task<IReadOnlyList<Store>> GetStoresAsync(CancellationToken ct = default);
}
