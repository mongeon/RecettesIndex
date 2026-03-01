using Microsoft.Extensions.Logging;
using RecettesIndex.Models;
using RecettesIndex.Services.Abstractions;
using RecettesIndex.Services.Exceptions;
using Supabase;
using Supabase.Postgrest.Interfaces;
using static Supabase.Postgrest.Constants;

namespace RecettesIndex.Services;

public class SupabaseRecipesQuery(Client supabaseClient, ILogger<SupabaseRecipesQuery> logger) : IRecipesQuery
{
    private readonly Client _supabaseClient = supabaseClient ?? throw new ArgumentNullException(nameof(supabaseClient));
    private readonly ILogger<SupabaseRecipesQuery> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Applies an optional rating filter to a recipe query. Only applies for valid ratings (1–5).
    /// </summary>
    private static IPostgrestTable<Recipe> ApplyRatingFilter(IPostgrestTable<Recipe> q, int? rating)
    {
        if (rating is >= 1 and <= 5)
        {
            q = q.Filter("rating", Operator.Equals, rating.Value);
        }

        return q;
    }

    public async Task<List<int>> GetRecipeIdsByNameAsync(string term, int? rating, CancellationToken ct = default)
    {
        try
        {
            var like = $"%{term}%";
            IPostgrestTable<Recipe> q = _supabaseClient.From<Recipe>();
            q = q.Filter("name", Operator.ILike, like);
            q = ApplyRatingFilter(q, rating);

            var res = await q.Get(cancellationToken: ct);
            return res.Models?.Select(r => r.Id).ToList() ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Network error while searching recipes by name: {Term}", term);
            throw new ServiceException("Network error. Please check your connection", ex);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error while searching recipes by name: {Term}", term);
            throw new ServiceException("An error occurred while searching recipes", ex);
        }
    }

    public async Task<List<int>> GetRecipeIdsByBookIdsAsync(IReadOnlyCollection<int> bookIds, int? rating, CancellationToken ct = default)
    {
        if (bookIds.Count == 0)
        {
            return [];
        }

        try
        {
            IPostgrestTable<Recipe> q = _supabaseClient.From<Recipe>();
            q = q.Filter("book_id", Operator.In, bookIds.ToList());
            q = ApplyRatingFilter(q, rating);

            var res = await q.Get(cancellationToken: ct);
            return res.Models?.Select(r => r.Id).ToList() ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Network error while fetching recipes by book IDs");
            throw new ServiceException("Network error. Please check your connection", ex);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error while fetching recipes by book IDs");
            throw new ServiceException("An unexpected error occurred while fetching recipes", ex);
        }
    }

    public async Task<List<int>> GetAllRecipeIdsAsync(int? rating, CancellationToken ct = default)
    {
        try
        {
            IPostgrestTable<Recipe> q = _supabaseClient.From<Recipe>().Select("id");
            q = ApplyRatingFilter(q, rating);

            var res = await q.Get(cancellationToken: ct);
            return res.Models?.Select(r => r.Id).ToList() ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Network error while fetching all recipe IDs");
            throw new ServiceException("Network error. Please check your connection", ex);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error while fetching all recipe IDs");
            throw new ServiceException("An unexpected error occurred while fetching recipes", ex);
        }
    }

    public async Task<IReadOnlyList<Recipe>> GetRecipeSummariesAsync(int? rating, CancellationToken ct = default)
    {
        try
        {
            IPostgrestTable<Recipe> q = _supabaseClient.From<Recipe>().Select("id,book_id,store_id,rating,created_at");
            q = ApplyRatingFilter(q, rating);

            var res = await q.Get(cancellationToken: ct);
            return (IReadOnlyList<Recipe>)(res.Models ?? []);
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Network error while fetching recipe summaries");
            throw new ServiceException("Network error. Please check your connection", ex);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error while fetching recipe summaries");
            throw new ServiceException("An unexpected error occurred while fetching recipe summaries", ex);
        }
    }

    public async Task<IReadOnlyList<Recipe>> GetRecipesByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default, string? sortColumn = null, bool sortDescending = false, int skip = 0, int take = 0)
    {
        if (ids.Count == 0)
        {
            return Array.Empty<Recipe>();
        }

        try
        {
            IPostgrestTable<Recipe> q = _supabaseClient.From<Recipe>().Filter("id", Operator.In, ids.ToList());

            if (!string.IsNullOrWhiteSpace(sortColumn))
            {
                q = q.Order(sortColumn, sortDescending ? Ordering.Descending : Ordering.Ascending);
            }

            if (take > 0)
            {
                q = q.Range(skip, skip + take - 1);
            }

            var res = await q.Get(cancellationToken: ct);
            return (IReadOnlyList<Recipe>)(res.Models ?? []);
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Network error while fetching recipes by IDs");
            throw new ServiceException("Network error. Please check your connection", ex);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error while fetching recipes by IDs");
            throw new ServiceException("An unexpected error occurred while fetching recipes", ex);
        }
    }

    public async Task<List<int>> GetBookIdsByTitleAsync(string term, CancellationToken ct = default)
    {
        try
        {
            var like = $"%{term}%";
            var res = await _supabaseClient.From<Book>().Filter("title", Operator.ILike, like).Get(cancellationToken: ct);
            return res.Models?.Select(b => b.Id).Distinct().ToList() ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Network error while searching books by title: {Term}", term);
            throw new ServiceException("Network error. Please check your connection", ex);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error while searching books by title: {Term}", term);
            throw new ServiceException("An error occurred while searching books", ex);
        }
    }

    public async Task<List<int>> GetBookIdsByAuthorIdsAsync(IReadOnlyCollection<int> authorIds, CancellationToken ct = default)
    {
        if (authorIds.Count == 0)
        {
            return [];
        }

        try
        {
            var res = await _supabaseClient.From<BookAuthor>().Filter("author_id", Operator.In, authorIds.ToList()).Get(cancellationToken: ct);
            return res.Models?.Select(ba => ba.BookId).Distinct().ToList() ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Network error while fetching books by author IDs");
            throw new ServiceException("Network error. Please check your connection", ex);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error while fetching books by author IDs");
            throw new ServiceException("An unexpected error occurred while fetching books", ex);
        }
    }

    public async Task<List<int>> GetBookIdsByAuthorAsync(int authorId, CancellationToken ct = default)
    {
        try
        {
            var res = await _supabaseClient.From<BookAuthor>().Filter("author_id", Operator.Equals, authorId).Get(cancellationToken: ct);
            return res.Models?.Select(ba => ba.BookId).Distinct().ToList() ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Network error while fetching books by author ID: {AuthorId}", authorId);
            throw new ServiceException("Network error. Please check your connection", ex);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error while fetching books by author ID: {AuthorId}", authorId);
            throw new ServiceException($"An error occurred while fetching books for author {authorId}", ex);
        }
    }

    public async Task<List<int>> GetAuthorIdsByNameAsync(string term, CancellationToken ct = default)
    {
        try
        {
            var like = $"%{term}%";
            var firstTask = _supabaseClient.From<Author>().Filter("first_name", Operator.ILike, like).Get(cancellationToken: ct);
            var lastTask = _supabaseClient.From<Author>().Filter("last_name", Operator.ILike, like).Get(cancellationToken: ct);
            await Task.WhenAll(firstTask, lastTask);
            var firstList = firstTask.Result.Models ?? [];
            var lastList = lastTask.Result.Models ?? [];
            return firstList.Concat(lastList).Select(a => a.Id).Distinct().ToList();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Network error while searching authors by name: {Term}", term);
            throw new ServiceException("Network error. Please check your connection", ex);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error while searching authors by name: {Term}", term);
            throw new ServiceException("An error occurred while searching authors", ex);
        }
    }

    public async Task<IReadOnlyList<Book>> GetBooksAsync(CancellationToken ct = default)
    {
        try
        {
            var res = await _supabaseClient.From<Book>().Get(cancellationToken: ct);
            return (IReadOnlyList<Book>)(res.Models ?? []);
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Network error while fetching all books");
            throw new ServiceException("Network error. Please check your connection", ex);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error while fetching all books");
            throw new ServiceException("An unexpected error occurred while fetching books", ex);
        }
    }

    public async Task<IReadOnlyList<Author>> GetAuthorsAsync(CancellationToken ct = default)
    {
        try
        {
            var res = await _supabaseClient.From<Author>().Get(cancellationToken: ct);
            return (IReadOnlyList<Author>)(res.Models ?? []);
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Network error while fetching all authors");
            throw new ServiceException("Network error. Please check your connection", ex);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error while fetching all authors");
            throw new ServiceException("An unexpected error occurred while fetching authors", ex);
        }
    }

    public async Task<List<int>> GetRecipeIdsByStoreIdsAsync(IReadOnlyCollection<int> storeIds, int? rating, CancellationToken ct = default)
    {
        if (storeIds.Count == 0)
        {
            return [];
        }

        try
        {
            IPostgrestTable<Recipe> q = _supabaseClient.From<Recipe>();
            q = q.Filter("store_id", Operator.In, storeIds.ToList());
            q = ApplyRatingFilter(q, rating);

            var res = await q.Get(cancellationToken: ct);
            return res.Models?.Select(r => r.Id).ToList() ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Network error while fetching recipes by store IDs");
            throw new ServiceException("Network error. Please check your connection", ex);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error while fetching recipes by store IDs");
            throw new ServiceException("An unexpected error occurred while fetching recipes", ex);
        }
    }

    public async Task<List<int>> GetStoreIdsByNameAsync(string term, CancellationToken ct = default)
    {
        try
        {
            var like = $"%{term}%";
            var res = await _supabaseClient.From<Store>().Filter("name", Operator.ILike, like).Get(cancellationToken: ct);
            return res.Models?.Select(s => s.Id).Distinct().ToList() ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Network error while searching stores by name: {Term}", term);
            throw new ServiceException("Network error. Please check your connection", ex);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error while searching stores by name: {Term}", term);
            throw new ServiceException("An error occurred while searching stores", ex);
        }
    }

    public async Task<IReadOnlyList<Store>> GetStoresAsync(CancellationToken ct = default)
    {
        try
        {
            var res = await _supabaseClient.From<Store>().Get(cancellationToken: ct);
            return (IReadOnlyList<Store>)(res.Models ?? []);
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Network error while fetching all stores");
            throw new ServiceException("Network error. Please check your connection", ex);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error while fetching all stores");
            throw new ServiceException("An unexpected error occurred while fetching stores", ex);
        }
    }
}


