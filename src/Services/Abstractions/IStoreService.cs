using RecettesIndex.Models;

namespace RecettesIndex.Services.Abstractions;

/// <summary>
/// Service for managing store/merchant/restaurant operations.
/// </summary>
public interface IStoreService
{
    /// <summary>
    /// Retrieves all stores.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of all stores.</returns>
    Task<IReadOnlyList<Store>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Retrieves a store by its unique identifier.
    /// </summary>
    /// <param name="id">The store ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing the store if found.</returns>
    Task<Result<Store>> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Creates a new store.
    /// </summary>
    /// <param name="store">The store to create.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing the created store.</returns>
    Task<Result<Store>> CreateAsync(Store store, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing store.
    /// </summary>
    /// <param name="store">The store with updated values.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing the updated store.</returns>
    Task<Result<Store>> UpdateAsync(Store store, CancellationToken ct = default);

    /// <summary>
    /// Deletes a store from the database.
    /// </summary>
    /// <param name="id">The ID of the store to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default);
}
