namespace RecettesIndex.Services.Abstractions;

/// <summary>
/// Abstraction over browser localStorage operations.
/// </summary>
public interface ILocalStorageService
{
    /// <summary>Gets a raw string value from localStorage.</summary>
    Task<string?> GetItemAsync(string key);

    /// <summary>Sets a raw string value in localStorage.</summary>
    Task SetItemAsync(string key, string value);

    /// <summary>Removes an item from localStorage.</summary>
    Task RemoveItemAsync(string key);

    /// <summary>Gets and deserializes a typed value from localStorage.</summary>
    Task<T?> GetItemAsync<T>(string key);

    /// <summary>Serializes and sets a typed value in localStorage.</summary>
    Task SetItemAsync<T>(string key, T value);
}
