using Microsoft.Extensions.Logging;
using RecettesIndex.Models;
using RecettesIndex.Services.Abstractions;

namespace RecettesIndex.Services;

/// <summary>
/// Service for managing store operations with caching and error handling.
/// </summary>
public class StoreService(
    ICacheService cache,
    Supabase.Client supabaseClient,
    ILogger<StoreService> logger) : IStoreService
{
    private readonly ICacheService _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly Supabase.Client _supabaseClient = supabaseClient ?? throw new ArgumentNullException(nameof(supabaseClient));
    private readonly ILogger<StoreService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<IReadOnlyList<Store>> GetAllAsync(CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheConstants.StoresListKey,
            CacheConstants.DefaultTtl,
            async ct =>
            {
                try
                {
                    var response = await _supabaseClient.From<Store>().Get(cancellationToken: ct);
                    return (IReadOnlyList<Store>)(response.Models ?? []);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading all stores");
                    return Array.Empty<Store>();
                }
            },
            ct);
    }

    public async Task<Result<Store>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var store = await _supabaseClient.From<Store>()
                .Where(x => x.Id == id)
                .Single();

            if (store == null)
            {
                return Result<Store>.Failure($"Store with ID {id} not found");
            }
            
            return Result<Store>.Success(store);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while getting store by id: {StoreId}", id);
            return Result<Store>.Failure("Network error. Please check your connection.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting store by id: {StoreId}", id);
            return Result<Store>.Failure("An unexpected error occurred while loading the store");
        }
    }

    public async Task<Result<Store>> CreateAsync(Store store, CancellationToken ct = default)
    {
        try
        {
            if (store == null)
            {
                return Result<Store>.Failure("Store cannot be null");
            }
            
            if (string.IsNullOrWhiteSpace(store.Name))
            {
                return Result<Store>.Failure("Store name is required");
            }

            store.CreationDate = DateTime.UtcNow;

            var response = await _supabaseClient.From<Store>().Insert(store);
            var createdStore = response.Models?.FirstOrDefault();

            if (createdStore == null)
            {
                return Result<Store>.Failure("Failed to create store");
            }

            _cache.Remove(CacheConstants.StoresListKey);

            _logger.LogInformation("Store created successfully: {StoreId}", createdStore.Id);
            return Result<Store>.Success(createdStore);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while creating store");
            return Result<Store>.Failure("Network error. Please check your connection.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating store");
            return Result<Store>.Failure("An unexpected error occurred while creating the store");
        }
    }

    public async Task<Result<Store>> UpdateAsync(Store store, CancellationToken ct = default)
    {
        try
        {
            if (store == null)
            {
                return Result<Store>.Failure("Store cannot be null");
            }
            
            if (string.IsNullOrWhiteSpace(store.Name))
            {
                return Result<Store>.Failure("Store name is required");
            }
            
            if (store.Id <= 0)
            {
                return Result<Store>.Failure("Invalid store ID");
            }

            // Use Upsert instead of Update for better compatibility
            var response = await _supabaseClient.From<Store>()
                .Upsert(store);
                
            var updatedStore = response.Models?.FirstOrDefault();

            if (updatedStore == null)
            {
                return Result<Store>.Failure("Failed to update store");
            }

            _cache.Remove(CacheConstants.StoresListKey);

            _logger.LogInformation("Store updated successfully: {StoreId}", updatedStore.Id);
            return Result<Store>.Success(updatedStore);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while updating store: {StoreId}", store.Id);
            return Result<Store>.Failure("Network error. Please check your connection.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating store: {StoreId}", store.Id);
            return Result<Store>.Failure("An unexpected error occurred while updating the store");
        }
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        try
        {
            if (id <= 0)
            {
                return Result<bool>.Failure("Invalid store ID");
            }

            var existingStore = await _supabaseClient.From<Store>()
                .Where(x => x.Id == id)
                .Single();

            if (existingStore == null)
            {
                return Result<bool>.Failure($"Store with ID {id} not found");
            }

            await _supabaseClient.From<Store>()
                .Where(x => x.Id == id)
                .Delete();

            _cache.Remove(CacheConstants.StoresListKey);

            _logger.LogInformation("Store deleted successfully: {StoreId}", id);
            return Result<bool>.Success(true);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while deleting store: {StoreId}", id);
            return Result<bool>.Failure("Network error. Please check your connection.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting store: {StoreId}", id);
            return Result<bool>.Failure("An unexpected error occurred while deleting the store");
        }
    }
}
