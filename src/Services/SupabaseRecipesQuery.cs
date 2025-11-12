using Microsoft.Extensions.Logging;
using RecettesIndex.Models;
using RecettesIndex.Services.Abstractions;
using RecettesIndex.Services.Exceptions;
using Supabase;
using Supabase.Postgrest.Interfaces;
using static Supabase.Postgrest.Constants;

namespace RecettesIndex.Services;

public class SupabaseRecipesQuery : IRecipesQuery
{
    private readonly Client _client;
    private readonly ILogger<SupabaseRecipesQuery>? _logger;
    
    public SupabaseRecipesQuery(Client client, ILogger<SupabaseRecipesQuery>? logger = null)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger;
    }

    public async Task<List<int>> GetRecipeIdsByNameAsync(string term, int? rating, CancellationToken ct = default)
    {
        try
        {
            var like = $"%{term}%";
            IPostgrestTable<Recipe> q = _client.From<Recipe>();
            q = q.Filter("name", Operator.ILike, like);
            if (rating is >= 1 and <= 5) q = q.Filter("rating", Operator.Equals, rating.Value);
            var res = await q.Get(cancellationToken: ct);
            return res.Models?.Select(r => r.Id).ToList() ?? new List<int>();
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
        if (bookIds.Count == 0) return new();
        
        try
        {
            IPostgrestTable<Recipe> q = _client.From<Recipe>();
            q = q.Filter("book_id", Operator.In, bookIds.ToList());
            if (rating is >= 1 and <= 5) q = q.Filter("rating", Operator.Equals, rating.Value);
            var res = await q.Get(cancellationToken: ct);
            return res.Models?.Select(r => r.Id).ToList() ?? new List<int>();
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
            IPostgrestTable<Recipe> q = _client.From<Recipe>();
            if (rating is >= 1 and <= 5) q = q.Filter("rating", Operator.Equals, rating.Value);
            var res = await q.Get(cancellationToken: ct);
            return res.Models?.Select(r => r.Id).ToList() ?? new List<int>();
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

    public async Task<IReadOnlyList<Recipe>> GetRecipesByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default)
    {
        if (ids.Count == 0) return Array.Empty<Recipe>();
        
        try
        {
            var res = await _client.From<Recipe>().Filter("id", Operator.In, ids.ToList()).Get(cancellationToken: ct);
            return (IReadOnlyList<Recipe>)(res.Models ?? new List<Recipe>());
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
            var res = await _client.From<Book>().Filter("title", Operator.ILike, like).Get(cancellationToken: ct);
            return res.Models?.Select(b => b.Id).Distinct().ToList() ?? new List<int>();
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
        if (authorIds.Count == 0) return new();
        
        try
        {
            var res = await _client.From<BookAuthor>().Filter("author_id", Operator.In, authorIds.ToList()).Get(cancellationToken: ct);
            return res.Models?.Select(ba => ba.BookId).Distinct().ToList() ?? new List<int>();
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
            var res = await _client.From<BookAuthor>().Filter("author_id", Operator.Equals, authorId).Get(cancellationToken: ct);
            return res.Models?.Select(ba => ba.BookId).Distinct().ToList() ?? new List<int>();
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
            var first = await _client.From<Author>().Filter("first_name", Operator.ILike, like).Get(cancellationToken: ct);
            var last = await _client.From<Author>().Filter("last_name", Operator.ILike, like).Get(cancellationToken: ct);
            var firstList = first.Models ?? new List<Author>();
            var lastList = last.Models ?? new List<Author>();
            return firstList.Concat(lastList).Select(a => a.Id).Distinct().ToList();
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
            var res = await _client.From<Book>().Get(cancellationToken: ct);
            return (IReadOnlyList<Book>)(res.Models ?? new List<Book>());
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
            var res = await _client.From<Author>().Get(cancellationToken: ct);
            return (IReadOnlyList<Author>)(res.Models ?? new List<Author>());
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
}

