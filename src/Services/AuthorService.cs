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
    ILogger<AuthorService> logger) : IAuthorService
{
    private readonly ICacheService _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly Supabase.Client _supabaseClient = supabaseClient ?? throw new ArgumentNullException(nameof(supabaseClient));
    private readonly ILogger<AuthorService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<IReadOnlyList<Author>> GetAllAsync(CancellationToken ct = default)
        => await _cache.GetOrEmptyAsync(
            CacheConstants.AuthorsListKey,
            CacheConstants.DefaultTtl,
            async token =>
            {
                var response = await _supabaseClient.From<Author>().Get(cancellationToken: token);
                return (IReadOnlyList<Author>)(response.Models ?? []);
            },
            _logger,
            ct);

    public async Task<Result<Author>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var author = await _supabaseClient.From<Author>()
                .Where(x => x.Id == id)
                .Single();

            if (author == null)
            {
                return Result<Author>.Failure($"Author with ID {id} not found");
            }

            return Result<Author>.Success(author);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while getting author by id: {AuthorId}", id);
            return Result<Author>.Failure("Network error. Please check your connection.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting author by id: {AuthorId}", id);
            return Result<Author>.Failure("An unexpected error occurred while loading the author");
        }
    }

    public async Task<Result<Author>> CreateAsync(Author author, CancellationToken ct = default)
    {
        try
        {
            var err = ValidationGuards.RequireNotNull(author, "Author");
            if (err != null) return Result<Author>.Failure(err);

            err = ValidationGuards.RequireNonEmpty(author.Name, "Author first name");
            if (err != null) return Result<Author>.Failure(err);

            author.CreationDate = DateTime.UtcNow;

            var response = await _supabaseClient.From<Author>().Insert(author);
            var createdAuthor = response.Models?.FirstOrDefault();

            if (createdAuthor == null)
            {
                return Result<Author>.Failure("Failed to create author");
            }

            _cache.Remove(CacheConstants.AuthorsListKey);

            _logger.LogInformation("Author created successfully: {AuthorId}", createdAuthor.Id);
            return Result<Author>.Success(createdAuthor);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while creating author");
            return Result<Author>.Failure("Network error. Please check your connection.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating author");
            return Result<Author>.Failure("An unexpected error occurred while creating the author");
        }
    }

    public async Task<Result<Author>> UpdateAsync(Author author, CancellationToken ct = default)
    {
        try
        {
            var err = ValidationGuards.RequireNotNull(author, "Author");
            if (err != null) return Result<Author>.Failure(err);

            err = ValidationGuards.RequireNonEmpty(author.Name, "Author first name");
            if (err != null) return Result<Author>.Failure(err);

            err = ValidationGuards.RequirePositive(author.Id, "author ID");
            if (err != null) return Result<Author>.Failure(err);

            var response = await _supabaseClient.From<Author>()
                .Where(x => x.Id == author.Id)
                .Update(author);

            var updatedAuthor = response.Models?.FirstOrDefault();

            if (updatedAuthor == null)
            {
                return Result<Author>.Failure("Failed to update author");
            }

            _cache.Remove(CacheConstants.AuthorsListKey);

            _logger.LogInformation("Author updated successfully: {AuthorId}", updatedAuthor.Id);
            return Result<Author>.Success(updatedAuthor);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while updating author: {AuthorId}", author.Id);
            return Result<Author>.Failure("Network error. Please check your connection.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating author: {AuthorId}", author.Id);
            return Result<Author>.Failure("An unexpected error occurred while updating the author");
        }
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var err = ValidationGuards.RequirePositive(id, "author ID");
            if (err != null) return Result<bool>.Failure(err);

            var existingAuthor = await _supabaseClient.From<Author>()
                .Where(x => x.Id == id)
                .Single();

            if (existingAuthor == null)
            {
                return Result<bool>.Failure($"Author with ID {id} not found");
            }

            await _supabaseClient.From<Author>()
                .Where(x => x.Id == id)
                .Delete();

            _cache.Remove(CacheConstants.AuthorsListKey);

            _logger.LogInformation("Author deleted successfully: {AuthorId}", id);
            return Result<bool>.Success(true);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while deleting author: {AuthorId}", id);
            return Result<bool>.Failure("Network error. Please check your connection.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting author: {AuthorId}", id);
            return Result<bool>.Failure("An unexpected error occurred while deleting the author");
        }
    }
}
