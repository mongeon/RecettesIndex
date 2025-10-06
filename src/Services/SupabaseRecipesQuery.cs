using RecettesIndex.Models;
using RecettesIndex.Services.Abstractions;
using Supabase;
using Supabase.Postgrest.Interfaces;
using static Supabase.Postgrest.Constants;

namespace RecettesIndex.Services;

public class SupabaseRecipesQuery : IRecipesQuery
{
    private readonly Client _client;
    public SupabaseRecipesQuery(Client client) => _client = client;

    public async Task<List<int>> GetRecipeIdsByNameAsync(string term, int? rating, CancellationToken ct = default)
    {
    var like = $"%{term}%";
    IPostgrestTable<Recipe> q = _client.From<Recipe>();
    q = q.Filter("name", Operator.ILike, like);
    if (rating is >= 1 and <= 5) q = q.Filter("rating", Operator.Equals, rating.Value);
    var res = await q.Get(cancellationToken: ct);
        return res.Models?.Select(r => r.Id).ToList() ?? new List<int>();
    }

    public async Task<List<int>> GetRecipeIdsByBookIdsAsync(IReadOnlyCollection<int> bookIds, int? rating, CancellationToken ct = default)
    {
        if (bookIds.Count == 0) return new();
    IPostgrestTable<Recipe> q = _client.From<Recipe>();
    q = q.Filter("book_id", Operator.In, bookIds.ToList());
    if (rating is >= 1 and <= 5) q = q.Filter("rating", Operator.Equals, rating.Value);
    var res = await q.Get(cancellationToken: ct);
        return res.Models?.Select(r => r.Id).ToList() ?? new List<int>();
    }

    public async Task<List<int>> GetAllRecipeIdsAsync(int? rating, CancellationToken ct = default)
    {
    IPostgrestTable<Recipe> q = _client.From<Recipe>();
    if (rating is >= 1 and <= 5) q = q.Filter("rating", Operator.Equals, rating.Value);
    var res = await q.Get(cancellationToken: ct);
        return res.Models?.Select(r => r.Id).ToList() ?? new List<int>();
    }

    public async Task<IReadOnlyList<Recipe>> GetRecipesByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default)
    {
        if (ids.Count == 0) return Array.Empty<Recipe>();
    var res = await _client.From<Recipe>().Filter("id", Operator.In, ids.ToList()).Get(cancellationToken: ct);
        return (IReadOnlyList<Recipe>)(res.Models ?? new List<Recipe>());
    }

    public async Task<List<int>> GetBookIdsByTitleAsync(string term, CancellationToken ct = default)
    {
        var like = $"%{term}%";
        var res = await _client.From<Book>().Filter("title", Operator.ILike, like).Get(cancellationToken: ct);
        return res.Models?.Select(b => b.Id).Distinct().ToList() ?? new List<int>();
    }

    public async Task<List<int>> GetBookIdsByAuthorIdsAsync(IReadOnlyCollection<int> authorIds, CancellationToken ct = default)
    {
        if (authorIds.Count == 0) return new();
        var res = await _client.From<BookAuthor>().Filter("author_id", Operator.In, authorIds.ToList()).Get(cancellationToken: ct);
        return res.Models?.Select(ba => ba.BookId).Distinct().ToList() ?? new List<int>();
    }

    public async Task<List<int>> GetBookIdsByAuthorAsync(int authorId, CancellationToken ct = default)
    {
        var res = await _client.From<BookAuthor>().Filter("author_id", Operator.Equals, authorId).Get(cancellationToken: ct);
        return res.Models?.Select(ba => ba.BookId).Distinct().ToList() ?? new List<int>();
    }

    public async Task<List<int>> GetAuthorIdsByNameAsync(string term, CancellationToken ct = default)
    {
        var like = $"%{term}%";
        var first = await _client.From<Author>().Filter("first_name", Operator.ILike, like).Get(cancellationToken: ct);
        var last = await _client.From<Author>().Filter("last_name", Operator.ILike, like).Get(cancellationToken: ct);
    var firstList = first.Models ?? new List<Author>();
    var lastList = last.Models ?? new List<Author>();
    return firstList.Concat(lastList).Select(a => a.Id).Distinct().ToList();
    }

    public async Task<IReadOnlyList<Book>> GetBooksAsync(CancellationToken ct = default)
    {
        var res = await _client.From<Book>().Get(cancellationToken: ct);
        return (IReadOnlyList<Book>)(res.Models ?? new List<Book>());
    }

    public async Task<IReadOnlyList<Author>> GetAuthorsAsync(CancellationToken ct = default)
    {
        var res = await _client.From<Author>().Get(cancellationToken: ct);
        return (IReadOnlyList<Author>)(res.Models ?? new List<Author>());
    }
}
