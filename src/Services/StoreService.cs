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
    ILogger<StoreService> logger) : CrudServiceBase<Store, StoreService>(cache, supabaseClient, logger), IStoreService
{
    public async Task<IReadOnlyList<Store>> GetAllAsync(CancellationToken ct = default)
        => await GetAllCachedAsync(
            CacheConstants.StoresListKey,
            async token =>
            {
                var response = await _supabaseClient.From<Store>().Get(cancellationToken: token);
                return (IReadOnlyList<Store>)(response.Models ?? []);
            },
            ct);

    public Task<Result<Store>> GetByIdAsync(int id, CancellationToken ct = default)
        => GetByIdCoreAsync(
            id,
            async () => await _supabaseClient.From<Store>().Where(x => x.Id == id).Single(),
            $"Store with ID {id} not found",
            "getting store by id",
            "An unexpected error occurred while loading the store");

    public Task<Result<Store>> CreateAsync(Store store, CancellationToken ct = default)
        => CreateCoreAsync(
            store,
            () =>
            {
                var err = ValidationGuards.RequireNotNull(store, "Store");
                if (err != null) return err;
                err = ValidationGuards.RequireNonEmpty(store.Name, "Store name");
                if (err != null) return err;
                return null;
            },
            async () =>
            {
                store.CreationDate = DateTime.UtcNow;
                var response = await _supabaseClient.From<Store>().Insert(store);
                return response.Models?.FirstOrDefault();
            },
            onSuccess: created =>
            {
                _cache.Remove(CacheConstants.StoresListKey);
                _logger.LogInformation("Store created successfully: {StoreId}", created.Id);
            },
            unexpectedUserMessage: "An unexpected error occurred while creating the store");

    public Task<Result<Store>> UpdateAsync(Store store, CancellationToken ct = default)
        => UpdateCoreAsync(
            store,
            () =>
            {
                var err = ValidationGuards.RequireNotNull(store, "Store");
                if (err != null) return err;
                err = ValidationGuards.RequireNonEmpty(store.Name, "Store name");
                if (err != null) return err;
                err = ValidationGuards.RequirePositive(store.Id, "store ID");
                if (err != null) return err;
                return null;
            },
            async () =>
            {
                var response = await _supabaseClient.From<Store>().Where(x => x.Id == store.Id).Update(store);
                return response.Models?.FirstOrDefault();
            },
            onSuccess: updated =>
            {
                _cache.Remove(CacheConstants.StoresListKey);
                _logger.LogInformation("Store updated successfully: {StoreId}", updated.Id);
            },
            unexpectedUserMessage: "An unexpected error occurred while updating the store",
            idForLogging: store?.Id,
            entityNameForLogging: "store");

    public Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
        => DeleteCoreAsync(
            id,
            async () => await _supabaseClient.From<Store>().Where(x => x.Id == id).Single(),
            async () => await _supabaseClient.From<Store>().Where(x => x.Id == id).Delete(),
            onSuccess: () =>
            {
                _cache.Remove(CacheConstants.StoresListKey);
                _logger.LogInformation("Store deleted successfully: {StoreId}", id);
            },
            notFoundMessage: $"Store with ID {id} not found",
            unexpectedUserMessage: "An unexpected error occurred while deleting the store",
            entityNameForLogging: "store");
}
