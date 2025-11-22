using Microsoft.Extensions.Logging;
using RecettesIndex.Models;
using RecettesIndex.Services.Abstractions;
using RecettesIndex.Services.Exceptions;

namespace RecettesIndex.Services;

/// <summary>
/// Service for managing recipe operations including search, CRUD operations, and related data retrieval.
/// </summary>
public class RecipeService(IRecipesQuery q, ICacheService cache, Supabase.Client supabaseClient, ILogger<RecipeService> logger) : IRecipeService
{
    private readonly IRecipesQuery _q = q ?? throw new ArgumentNullException(nameof(q));
    private readonly ICacheService _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly Supabase.Client _supabaseClient = supabaseClient ?? throw new ArgumentNullException(nameof(supabaseClient));
    private readonly ILogger<RecipeService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Searches for recipes based on various criteria with pagination and sorting support.
    /// </summary>
    /// <param name="term">Search term to filter by recipe name, book title, or author name.</param>
    /// <param name="rating">Optional rating filter (1-5).</param>
    /// <param name="bookId">Optional book ID filter.</param>
    /// <param name="authorId">Optional author ID filter.</param>
    /// <param name="page">Page number for pagination (1-based).</param>
    /// <param name="pageSize">Number of items per page (clamped between 1-100).</param>
    /// <param name="sortLabel">Column to sort by (name, rating, created_at).</param>
    /// <param name="sortDescending">Whether to sort in descending order.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing a tuple of recipe list and total count.</returns>
    public async Task<Result<(IReadOnlyList<Recipe> Items, int Total)>> SearchAsync(string? term, int? rating, int? bookId, int? authorId, int page, int pageSize, string? sortLabel = null, bool sortDescending = false, CancellationToken ct = default)
    {
        try
        {
            page = Math.Max(PaginationConstants.MinPage, page);
            pageSize = Math.Clamp(pageSize, PaginationConstants.MinPageSize, PaginationConstants.MaxPageSize);

            var ids = new HashSet<int>();

            if (!string.IsNullOrWhiteSpace(term))
            {
                foreach (var id in await _q.GetRecipeIdsByNameAsync(term.Trim(), rating, ct))
                {
                    ids.Add(id);
                }
                var bookIdsByTitle = await _q.GetBookIdsByTitleAsync(term.Trim(), ct);
                foreach (var id in await _q.GetRecipeIdsByBookIdsAsync(bookIdsByTitle, rating, ct))
                {
                    ids.Add(id);
                }
                var authorIds = await _q.GetAuthorIdsByNameAsync(term.Trim(), ct);
                var bookIdsByAuthors = await _q.GetBookIdsByAuthorIdsAsync(authorIds, ct);
                foreach (var id in await _q.GetRecipeIdsByBookIdsAsync(bookIdsByAuthors, rating, ct))
                {
                    ids.Add(id);
                }
            }
            else
            {
                foreach (var id in await _q.GetAllRecipeIdsAsync(rating, ct))
                {
                    ids.Add(id);
                }
            }

            if (bookId.HasValue)
            {
                var idsByBook = await _q.GetRecipeIdsByBookIdsAsync([bookId.Value], rating, ct);
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
                    RecipeSortConstants.Name => sortDescending ? allModels.OrderByDescending(r => r.Name) : allModels.OrderBy(r => r.Name),
                    RecipeSortConstants.Rating => sortDescending ? allModels.OrderByDescending(r => r.Rating) : allModels.OrderBy(r => r.Rating),
                    RecipeSortConstants.CreatedAt => sortDescending ? allModels.OrderByDescending(r => r.CreationDate) : allModels.OrderBy(r => r.CreationDate),
                    _ => allModels
                };
            }

            // Apply pagination after sorting
            var skip = (page - 1) * pageSize;
            var pagedModels = sortedModels.Skip(skip).Take(pageSize).ToList();

            (IReadOnlyList<Recipe> Items, int Total) payload = (pagedModels, total);
            return Result<(IReadOnlyList<Recipe> Items, int Total)>.Success(payload);
        }
        catch (ServiceException ex)
        {
            _logger?.LogError(ex, "Service error during recipe search");
            return Result<(IReadOnlyList<Recipe> Items, int Total)>.Failure(ex.Message);
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Network error during recipe search");
            return Result<(IReadOnlyList<Recipe> Items, int Total)>.Failure("Network error. Please check your connection.");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error during recipe search");
            return Result<(IReadOnlyList<Recipe> Items, int Total)>.Failure("An unexpected error occurred while searching recipes");
        }
    }

    /// <summary>
    /// Retrieves a recipe by its unique identifier.
    /// </summary>
    /// <param name="id">The recipe ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing the recipe if found, or a failure result.</returns>
    public async Task<Result<Recipe>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var list = await _q.GetRecipesByIdsAsync(new[] { id }, ct);
            var model = list.FirstOrDefault();
            if (model is null)
            {
                throw new NotFoundException("Recipe", id);
            }
            return Result<Recipe>.Success(model);
        }
        catch (NotFoundException ex)
        {
            _logger?.LogWarning("Recipe not found: {RecipeId}", id);
            return Result<Recipe>.Failure(ex.Message);
        }
        catch (ServiceException ex)
        {
            _logger?.LogError(ex, "Service error while getting recipe by id: {RecipeId}", id);
            return Result<Recipe>.Failure(ex.Message);
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Network error while getting recipe by id: {RecipeId}", id);
            return Result<Recipe>.Failure("Network error. Please check your connection.");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error while getting recipe by id: {RecipeId}", id);
            return Result<Recipe>.Failure("An unexpected error occurred while loading the recipe");
        }
    }

    /// <summary>
    /// Creates a new recipe in the database.
    /// </summary>
    /// <param name="recipe">The recipe to create.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing the created recipe with its generated ID.</returns>
    public async Task<Result<Recipe>> CreateAsync(Recipe recipe, CancellationToken ct = default)
    {
        try
        {
            // Input validation
            if (recipe == null)
            {
                return Result<Recipe>.Failure("Recipe cannot be null");
            }
            
            if (string.IsNullOrWhiteSpace(recipe.Name))
            {
                return Result<Recipe>.Failure("Recipe name is required");
            }
            
            if (recipe.Rating < 0 || recipe.Rating > 5)
            {
                return Result<Recipe>.Failure("Rating must be between 0 and 5");
            }
            
            if (recipe.BookPage.HasValue && recipe.BookPage.Value <= 0)
            {
                return Result<Recipe>.Failure("Book page number must be positive");
            }

            // Set creation date if not already set
            if (recipe.CreationDate == default)
            {
                recipe.CreationDate = DateTime.UtcNow;
            }

            var res = await _supabaseClient.From<Recipe>().Insert(recipe);
            var created = res.Models?.FirstOrDefault() ?? recipe;

            // Invalidate related caches
            InvalidateRelatedCaches();

            _logger?.LogInformation("Recipe created successfully: {RecipeId}", created.Id);
            return Result<Recipe>.Success(created);
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Network error while creating recipe");
            return Result<Recipe>.Failure("Network error. Please check your connection.");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error while creating recipe");
            return Result<Recipe>.Failure("An unexpected error occurred while creating the recipe");
        }
    }

    /// <summary>
    /// Updates an existing recipe in the database.
    /// </summary>
    /// <param name="recipe">The recipe with updated values.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing the updated recipe.</returns>
    public async Task<Result<Recipe>> UpdateAsync(Recipe recipe, CancellationToken ct = default)
    {
        try
        {
            // Input validation
            if (recipe == null)
            {
                return Result<Recipe>.Failure("Recipe cannot be null");
            }
            
            if (recipe.Id <= 0)
            {
                return Result<Recipe>.Failure("Invalid recipe ID");
            }
            
            if (string.IsNullOrWhiteSpace(recipe.Name))
            {
                return Result<Recipe>.Failure("Recipe name is required");
            }
            
            if (recipe.Rating < 0 || recipe.Rating > 5)
            {
                return Result<Recipe>.Failure("Rating must be between 0 and 5");
            }
            
            if (recipe.BookPage.HasValue && recipe.BookPage.Value <= 0)
            {
                return Result<Recipe>.Failure("Book page number must be positive");
            }

            var res = await _supabaseClient.From<Recipe>().Update(recipe);
            var updated = res.Models?.FirstOrDefault() ?? recipe;

            // Invalidate related caches
            InvalidateRelatedCaches();

            _logger?.LogInformation("Recipe updated successfully: {RecipeId}", updated.Id);
            return Result<Recipe>.Success(updated);
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Network error while updating recipe: {RecipeId}", recipe.Id);
            return Result<Recipe>.Failure("Network error. Please check your connection.");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error while updating recipe: {RecipeId}", recipe.Id);
            return Result<Recipe>.Failure("An unexpected error occurred while updating the recipe");
        }
    }

    /// <summary>
    /// Deletes a recipe from the database.
    /// </summary>
    /// <param name="id">The ID of the recipe to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        try
        {
            // Input validation
            if (id <= 0)
            {
                return Result<bool>.Failure("Invalid recipe ID");
            }

            _logger?.LogInformation("Attempting to delete recipe with ID: {RecipeId}", id);
            
            // First, verify the recipe exists
            var existingRecipe = await _supabaseClient.From<Recipe>()
                .Where(x => x.Id == id)
                .Single();
            
            if (existingRecipe == null)
            {
                _logger?.LogWarning("Recipe with ID {RecipeId} not found", id);
                return Result<bool>.Failure($"Recipe with ID {id} not found");
            }
            
            // Delete the recipe
            await _supabaseClient.From<Recipe>()
                .Where(x => x.Id == id)
                .Delete();
            
            _logger?.LogInformation("Recipe {RecipeId} deleted successfully", id);
            
            // Invalidate related caches
            InvalidateRelatedCaches();

            return Result<bool>.Success(true);
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Network error while deleting recipe: {RecipeId}", id);
            return Result<bool>.Failure("Network error. Please check your connection.");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error while deleting recipe: {RecipeId}", id);
            return Result<bool>.Failure($"An unexpected error occurred while deleting the recipe: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves all books, cached for improved performance.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of all books.</returns>
    public Task<IReadOnlyList<Book>> GetBooksAsync(CancellationToken ct = default)
        => _cache.GetOrCreateAsync(CacheConstants.BooksListKey, CacheConstants.DefaultTtl, async _ => await _q.GetBooksAsync(ct), ct);

    /// <summary>
    /// Retrieves all authors, cached for improved performance.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of all authors.</returns>
    public Task<IReadOnlyList<Author>> GetAuthorsAsync(CancellationToken ct = default)
        => _cache.GetOrCreateAsync(CacheConstants.AuthorsListKey, CacheConstants.DefaultTtl, async _ => await _q.GetAuthorsAsync(ct), ct);

    /// <summary>
    /// Invalidates related caches when recipes are modified.
    /// </summary>
    private void InvalidateRelatedCaches()
    {
        // Note: Books and authors lists rarely change based on recipe modifications,
        // but we invalidate them to ensure consistency if book/author relationships change
        _cache.Remove(CacheConstants.BooksListKey);
        _cache.Remove(CacheConstants.AuthorsListKey);
    }
}
