using RecettesIndex.Models;

namespace RecettesIndex.Services.Abstractions;

public interface IRecipeService
{
    public Task<Result<(IReadOnlyList<Recipe> Items, int Total)>> SearchAsync(string? term, int? rating, int? bookId, int? storeId, int? authorId, int page, int pageSize, string? sortLabel = null, bool sortDescending = false, CancellationToken ct = default);
    public Task<Result<Recipe>> GetByIdAsync(int id, CancellationToken ct = default);
    public Task<Result<Recipe>> CreateAsync(Recipe recipe, CancellationToken ct = default);
    public Task<Result<Recipe>> UpdateAsync(Recipe recipe, CancellationToken ct = default);
    public Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default);
    // Helper data for filters
    public Task<IReadOnlyList<Book>> GetBooksAsync(CancellationToken ct = default);
    public Task<IReadOnlyList<Author>> GetAuthorsAsync(CancellationToken ct = default);
    public Task<IReadOnlyList<Store>> GetStoresAsync(CancellationToken ct = default);
}
