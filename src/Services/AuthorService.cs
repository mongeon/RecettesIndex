using Microsoft.Extensions.Logging;
using RecettesIndex.Models;
using RecettesIndex.Services.Abstractions;

namespace RecettesIndex.Services;

/// <summary>
/// Service for managing author operations with caching and error handling.
/// </summary>
public class AuthorService(
    ICacheService cache,
    Supabase.Client supabaseClient,
    ILogger<AuthorService> logger) : CrudServiceBase<Author, AuthorService>(cache, supabaseClient, logger), IAuthorService
{
    public async Task<IReadOnlyList<Author>> GetAllAsync(CancellationToken ct = default)
        => await GetAllCachedAsync(
            CacheConstants.AuthorsListKey,
            async token =>
            {
                var response = await _supabaseClient.From<Author>().Get(cancellationToken: token);
                return (IReadOnlyList<Author>)(response.Models ?? []);
            },
            ct);

    public Task<Result<Author>> GetByIdAsync(int id, CancellationToken ct = default)
        => GetByIdCoreAsync(
            id,
            async () => await _supabaseClient.From<Author>().Where(x => x.Id == id).Single(),
            $"Author with ID {id} not found",
            "getting author by id",
            "An unexpected error occurred while loading the author");

    public Task<Result<Author>> CreateAsync(Author author, CancellationToken ct = default)
        => CreateCoreAsync(
            author,
            () =>
            {
                var err = ValidationGuards.RequireNotNull(author, "Author");
                if (err != null) return err;
                err = ValidationGuards.RequireNonEmpty(author.Name, "Author first name");
                if (err != null) return err;
                return null;
            },
            async () =>
            {
                author.CreationDate = DateTime.UtcNow;
                var response = await _supabaseClient.From<Author>().Insert(author);
                return response.Models?.FirstOrDefault();
            },
            onSuccess: created =>
            {
                _cache.Remove(CacheConstants.AuthorsListKey);
                _logger.LogInformation("Author created successfully: {AuthorId}", created.Id);
            },
            unexpectedUserMessage: "An unexpected error occurred while creating the author");

    public Task<Result<Author>> UpdateAsync(Author author, CancellationToken ct = default)
        => UpdateCoreAsync(
            author,
            () =>
            {
                var err = ValidationGuards.RequireNotNull(author, "Author");
                if (err != null) return err;
                err = ValidationGuards.RequireNonEmpty(author.Name, "Author first name");
                if (err != null) return err;
                err = ValidationGuards.RequirePositive(author.Id, "author ID");
                if (err != null) return err;
                return null;
            },
            async () =>
            {
                var response = await _supabaseClient.From<Author>().Where(x => x.Id == author.Id).Update(author);
                return response.Models?.FirstOrDefault();
            },
            onSuccess: updated =>
            {
                _cache.Remove(CacheConstants.AuthorsListKey);
                _logger.LogInformation("Author updated successfully: {AuthorId}", updated.Id);
            },
            unexpectedUserMessage: "An unexpected error occurred while updating the author",
            idForLogging: author?.Id,
            entityNameForLogging: "author");

    public Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
        => DeleteCoreAsync(
            id,
            async () => await _supabaseClient.From<Author>().Where(x => x.Id == id).Single(),
            async () => await _supabaseClient.From<Author>().Where(x => x.Id == id).Delete(),
            onSuccess: () =>
            {
                _cache.Remove(CacheConstants.AuthorsListKey);
                _logger.LogInformation("Author deleted successfully: {AuthorId}", id);
            },
            notFoundMessage: $"Author with ID {id} not found",
            unexpectedUserMessage: "An unexpected error occurred while deleting the author",
            entityNameForLogging: "author");
}
