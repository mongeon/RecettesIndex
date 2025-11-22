using Microsoft.JSInterop;
using System.Text.Json;

namespace RecettesIndex.Services;

/// <summary>
/// Service for managing browser localStorage operations.
/// Provides reusable methods for storing and retrieving data from localStorage.
/// </summary>
public class LocalStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
    }

    /// <summary>
    /// Gets a value from localStorage.
    /// </summary>
    public async Task<string?> GetItemAsync(string key)
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Sets a value in localStorage.
    /// </summary>
    public async Task SetItemAsync(string key, string value)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
        }
        catch
        {
            // Silently fail if localStorage is not available
        }
    }

    /// <summary>
    /// Removes an item from localStorage.
    /// </summary>
    public async Task RemoveItemAsync(string key)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
        catch
        {
            // Silently fail
        }
    }

    /// <summary>
    /// Gets a typed object from localStorage.
    /// </summary>
    public async Task<T?> GetItemAsync<T>(string key)
    {
        var json = await GetItemAsync(key);
        if (string.IsNullOrEmpty(json))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Sets a typed object in localStorage.
    /// </summary>
    public async Task SetItemAsync<T>(string key, T value)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await SetItemAsync(key, json);
        }
        catch
        {
            // Silently fail
        }
    }
}

/// <summary>
/// Represents a recently viewed recipe.
/// </summary>
public class RecentRecipe
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime ViewedAt { get; set; }
}
