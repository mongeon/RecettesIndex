using Microsoft.Extensions.Logging;
using RecettesIndex.Models;
using RecettesIndex.Services.Abstractions;

namespace RecettesIndex.Services;

public class RecipeService : IRecipeService
{
    private readonly IRecipesQuery _q;
    private readonly ILogger<RecipeService>? _logger;
    private readonly ICacheService _cache;
    private readonly Supabase.Client _client;

    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(3);

    public RecipeService(IRecipesQuery q, ICacheService cache, Supabase.Client client, ILogger<RecipeService>? logger = null)
    {
        _q = q ?? throw new ArgumentNullException(nameof(q));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger;
    }

    public async Task<Result<(IReadOnlyList<Recipe> Items, int Total)>> SearchAsync(string? term, int? rating, int? bookId, int? authorId, int page, int pageSize, string? sortLabel = null, bool sortDescending = false, CancellationToken ct = default)
    {
        try
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var ids = new HashSet<int>();

            if (!string.IsNullOrWhiteSpace(term))
            {
                foreach (var id in await _q.GetRecipeIdsByNameAsync(term.Trim(), rating, ct)) ids.Add(id);
                var bookIdsByTitle = await _q.GetBookIdsByTitleAsync(term.Trim(), ct);
                foreach (var id in await _q.GetRecipeIdsByBookIdsAsync(bookIdsByTitle, rating, ct)) ids.Add(id);
                var authorIds = await _q.GetAuthorIdsByNameAsync(term.Trim(), ct);
                var bookIdsByAuthors = await _q.GetBookIdsByAuthorIdsAsync(authorIds, ct);
                foreach (var id in await _q.GetRecipeIdsByBookIdsAsync(bookIdsByAuthors, rating, ct)) ids.Add(id);
            }
            else
            {
                foreach (var id in await _q.GetAllRecipeIdsAsync(rating, ct)) ids.Add(id);
            }

            if (bookId.HasValue)
            {
                var idsByBook = await _q.GetRecipeIdsByBookIdsAsync(new[] { bookId.Value }, rating, ct);
                ids.IntersectWith(idsByBook);
            }

            if (authorId.HasValue)
            {
                var bookIds = await _q.GetBookIdsByAuthorAsync(authorId.Value, ct);
                var idsByAuthor = await _q.GetRecipeIdsByBookIdsAsync(bookIds, rating, ct);
                ids.IntersectWith(idsByAuthor);
            }

            var total = ids.Count;
            
            // Get all recipes for sorting
            var allModels = await _q.GetRecipesByIdsAsync(ids.ToList(), ct);
            
            // Apply sorting
            IEnumerable<Recipe> sortedModels = allModels;
            if (!string.IsNullOrWhiteSpace(sortLabel))
            {
                sortedModels = sortLabel.ToLower() switch
                {
                    "name" => sortDescending ? allModels.OrderByDescending(r => r.Name) : allModels.OrderBy(r => r.Name),
                    "rating" => sortDescending ? allModels.OrderByDescending(r => r.Rating) : allModels.OrderBy(r => r.Rating),
                    "created_at" => sortDescending ? allModels.OrderByDescending(r => r.CreationDate) : allModels.OrderBy(r => r.CreationDate),
                    _ => allModels
                };
            }
            
            // Apply pagination after sorting
            var skip = (page - 1) * pageSize;
            var pagedModels = sortedModels.Skip(skip).Take(pageSize).ToList();
            
            (IReadOnlyList<Recipe> Items, int Total) payload = (pagedModels, total);
            return Result<(IReadOnlyList<Recipe> Items, int Total)>.Success(payload);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Recipe search failed");
            return Result<(IReadOnlyList<Recipe> Items, int Total)>.Failure("Failed to load recipes");
        }
    }

    public async Task<Result<Recipe>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var list = await _q.GetRecipesByIdsAsync(new[] { id }, ct);
            var model = list.FirstOrDefault();
            if (model is null) return Result<Recipe>.Failure("Not found");
            return Result<Recipe>.Success(model);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Get recipe by id failed: {RecipeId}", id);
            return Result<Recipe>.Failure("Failed to load recipe");
        }
    }

    public async Task<Result<Recipe>> CreateAsync(Recipe recipe, CancellationToken ct = default)
    {
        try
        {
            var res = await _client.From<Recipe>().Insert(recipe);
            var created = res.Models?.FirstOrDefault() ?? recipe;
            return Result<Recipe>.Success(created);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Create recipe failed");
            return Result<Recipe>.Failure("Failed to create recipe");
        }
    }

    public async Task<Result<Recipe>> UpdateAsync(Recipe recipe, CancellationToken ct = default)
    {
        try
        {
            var res = await _client.From<Recipe>().Update(recipe);
            var updated = res.Models?.FirstOrDefault() ?? recipe;
            return Result<Recipe>.Success(updated);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Update recipe failed: {RecipeId}", recipe.Id);
            return Result<Recipe>.Failure("Failed to update recipe");
        }
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        try
        {
            await _client.From<Recipe>().Where(x => x.Id == id).Delete();
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Delete recipe failed: {RecipeId}", id);
            return Result<bool>.Failure("Failed to delete recipe");
        }
    }

    public Task<IReadOnlyList<Book>> GetBooksAsync(CancellationToken ct = default)
        => _cache.GetOrCreateAsync("books:list", CacheTtl, async _ => await _q.GetBooksAsync(ct), ct);

    public Task<IReadOnlyList<Author>> GetAuthorsAsync(CancellationToken ct = default)
        => _cache.GetOrCreateAsync("authors:list", CacheTtl, async _ => await _q.GetAuthorsAsync(ct), ct);
}
