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
    {
        return await _cache.GetOrCreateAsync(
            CacheConstants.AuthorsListKey,
            CacheConstants.DefaultTtl,
            async ct =>
            {
                try
                {
                    var response = await _supabaseClient.From<Author>().Get(cancellationToken: ct);
                    return (IReadOnlyList<Author>)(response.Models ?? []);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading all authors");
                    return Array.Empty<Author>();
                }
            },
            ct);
    }

    public async Task<Result<Author>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var author = await _supabaseClient.From<Author>()
                .Where(x => x.Id == id)
                .Single();

            if (author == null)
                return Result<Author>.Failure($"Author with ID {id} not found");
            
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
            // Validate input
            if (author == null)
                return Result<Author>.Failure("Author cannot be null");
            
            if (string.IsNullOrWhiteSpace(author.Name))
                return Result<Author>.Failure("Author first name is required");

            // Set creation date
            author.CreationDate = DateTime.UtcNow;

            // Insert author
            var response = await _supabaseClient.From<Author>().Insert(author);
            var createdAuthor = response.Models?.FirstOrDefault();

            if (createdAuthor == null)
                return Result<Author>.Failure("Failed to create author");

            // Invalidate cache
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
            // Validate input
            if (author == null)
                return Result<Author>.Failure("Author cannot be null");
            
            if (string.IsNullOrWhiteSpace(author.Name))
                return Result<Author>.Failure("Author first name is required");
            
            if (author.Id <= 0)
                return Result<Author>.Failure("Invalid author ID");

            // Update author
            var response = await _supabaseClient.From<Author>()
                .Where(x => x.Id == author.Id)
                .Update(author);
                
            var updatedAuthor = response.Models?.FirstOrDefault();

            if (updatedAuthor == null)
                return Result<Author>.Failure("Failed to update author");

            // Invalidate cache
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
            // Validate input
            if (id <= 0)
                return Result<bool>.Failure("Invalid author ID");

            // Check if author exists
            var existingAuthor = await _supabaseClient.From<Author>()
                .Where(x => x.Id == id)
                .Single();

            if (existingAuthor == null)
                return Result<bool>.Failure($"Author with ID {id} not found");

            // Delete author
            await _supabaseClient.From<Author>()
                .Where(x => x.Id == id)
                .Delete();

            // Invalidate cache
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
