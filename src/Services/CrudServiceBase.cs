using Microsoft.Extensions.Logging;
using RecettesIndex.Services.Abstractions;

namespace RecettesIndex.Services;

/// <summary>
/// Small CRUD helper base class to reduce duplication across services.
/// Provides common patterns for list retrieval, single fetch, create, update, delete with
/// consistent logging, error messages, and cache invalidation hooks.
/// </summary>
/// <typeparam name="TModel">The entity model type.</typeparam>
/// <typeparam name="TService">The concrete service type for logger.</typeparam>
public abstract class CrudServiceBase<TModel, TService>
{
    protected readonly ICacheService _cache;
    protected readonly Supabase.Client _supabaseClient;
    protected readonly ILogger<TService> _logger;

    protected CrudServiceBase(ICacheService cache, Supabase.Client supabaseClient, ILogger<TService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _supabaseClient = supabaseClient ?? throw new ArgumentNullException(nameof(supabaseClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected Task<IReadOnlyList<TModel>> GetAllCachedAsync(
        string cacheKey,
        Func<CancellationToken, Task<IReadOnlyList<TModel>>> fetch,
        CancellationToken ct = default)
        => _cache.GetOrEmptyAsync(cacheKey, CacheConstants.DefaultTtl, fetch, _logger, ct);

    protected async Task<Result<TModel>> GetByIdCoreAsync(
        int id,
        Func<Task<TModel?>> fetchSingle,
        string notFoundMessage,
        string logContext,
        string unexpectedUserMessage)
    {
        try
        {
            var model = await fetchSingle();
            if (model == null)
            {
                return Result<TModel>.Failure(notFoundMessage);
            }
            return Result<TModel>.Success(model);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while {Context}: {Id}", logContext, id);
            return Result<TModel>.Failure("Network error. Please check your connection.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while {Context}: {Id}", logContext, id);
            return Result<TModel>.Failure(unexpectedUserMessage);
        }
    }

    protected async Task<Result<TModel>> CreateCoreAsync(
        TModel entity,
        Func<string?> validate,
        Func<Task<TModel?>> doInsert,
        Action<TModel>? onSuccess,
        string unexpectedUserMessage)
    {
        try
        {
            var err = validate();
            if (err != null)
                return Result<TModel>.Failure(err);

            var created = await doInsert();
            if (created == null)
                return Result<TModel>.Failure("Failed to create entity");

            onSuccess?.Invoke(created);
            return Result<TModel>.Success(created);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while creating entity");
            return Result<TModel>.Failure("Network error. Please check your connection.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating entity");
            return Result<TModel>.Failure(unexpectedUserMessage);
        }
    }

    protected async Task<Result<TModel>> UpdateCoreAsync(
        TModel entity,
        Func<string?> validate,
        Func<Task<TModel?>> doUpdate,
        Action<TModel>? onSuccess,
        string unexpectedUserMessage,
        int? idForLogging = null,
        string? entityNameForLogging = null)
    {
        try
        {
            var err = validate();
            if (err != null)
                return Result<TModel>.Failure(err);

            var updated = await doUpdate();
            if (updated == null)
                return Result<TModel>.Failure("Failed to update entity");

            onSuccess?.Invoke(updated);
            return Result<TModel>.Success(updated);
        }
        catch (HttpRequestException ex)
        {
            if (idForLogging.HasValue)
                _logger.LogError(ex, "Network error while updating {Entity}: {Id}", entityNameForLogging ?? "entity", idForLogging.Value);
            else
                _logger.LogError(ex, "Network error while updating entity");
            return Result<TModel>.Failure("Network error. Please check your connection.");
        }
        catch (Exception ex)
        {
            if (idForLogging.HasValue)
                _logger.LogError(ex, "Error while updating {Entity}: {Id}", entityNameForLogging ?? "entity", idForLogging.Value);
            else
                _logger.LogError(ex, "Error while updating entity");
            return Result<TModel>.Failure(unexpectedUserMessage);
        }
    }

    protected async Task<Result<bool>> DeleteCoreAsync(
        int id,
        Func<Task<TModel?>> getExisting,
        Func<Task> doDelete,
        Action? onSuccess,
        string notFoundMessage,
        string unexpectedUserMessage,
        string entityNameForLogging)
    {
        try
        {
            var err = ValidationGuards.RequirePositive(id, $"{entityNameForLogging.ToLower()} ID");
            if (err != null)
                return Result<bool>.Failure(err);

            var existing = await getExisting();
            if (existing == null)
            {
                return Result<bool>.Failure(notFoundMessage);
            }

            await doDelete();
            onSuccess?.Invoke();
            return Result<bool>.Success(true);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while deleting {Entity}: {Id}", entityNameForLogging, id);
            return Result<bool>.Failure("Network error. Please check your connection.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting {Entity}: {Id}", entityNameForLogging, id);
            return Result<bool>.Failure(unexpectedUserMessage);
        }
    }
}
