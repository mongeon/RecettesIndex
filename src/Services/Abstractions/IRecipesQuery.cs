using RecettesIndex.Models;

namespace RecettesIndex.Services.Abstractions;

public interface IRecipesQuery
{
    Task<List<int>> GetRecipeIdsByNameAsync(string term, int? rating, CancellationToken ct = default);
    Task<List<int>> GetRecipeIdsByBookIdsAsync(IReadOnlyCollection<int> bookIds, int? rating, CancellationToken ct = default);
    Task<List<int>> GetAllRecipeIdsAsync(int? rating, CancellationToken ct = default);
    Task<IReadOnlyList<Recipe>> GetRecipesByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);

    Task<List<int>> GetBookIdsByTitleAsync(string term, CancellationToken ct = default);
    Task<List<int>> GetBookIdsByAuthorIdsAsync(IReadOnlyCollection<int> authorIds, CancellationToken ct = default);
    Task<List<int>> GetBookIdsByAuthorAsync(int authorId, CancellationToken ct = default);
    Task<List<int>> GetAuthorIdsByNameAsync(string term, CancellationToken ct = default);

    Task<IReadOnlyList<Book>> GetBooksAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Author>> GetAuthorsAsync(CancellationToken ct = default);
}
