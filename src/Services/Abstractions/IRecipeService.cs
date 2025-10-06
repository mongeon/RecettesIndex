using RecettesIndex.Models;

namespace RecettesIndex.Services.Abstractions;

public interface IRecipeService
{
    Task<Result<(IReadOnlyList<Recipe> Items, int Total)>> SearchAsync(string? term, int? rating, int? bookId, int? authorId, int page, int pageSize, CancellationToken ct = default);
    Task<Result<Recipe>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<Recipe>> CreateAsync(Recipe recipe, CancellationToken ct = default);
    Task<Result<Recipe>> UpdateAsync(Recipe recipe, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default);
    // Helper data for filters
    Task<IReadOnlyList<Book>> GetBooksAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Author>> GetAuthorsAsync(CancellationToken ct = default);
}
