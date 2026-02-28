using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using RecettesIndex.Services.Abstractions;

namespace RecettesIndex.Services;

/// <summary>
/// Service for managing browser localStorage operations.
/// Provides reusable methods for storing and retrieving data from localStorage.
/// </summary>
public class LocalStorageService : ILocalStorageService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<LocalStorageService> _logger;

    public LocalStorageService(IJSRuntime jsRuntime, ILogger<LocalStorageService> logger)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "localStorage.getItem failed for key '{Key}'", key);
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
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "localStorage.setItem failed for key '{Key}'", key);
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
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "localStorage.removeItem failed for key '{Key}'", key);
        }
    }

    /// <summary>
    /// Gets a typed object from localStorage.
    /// </summary>
    public async Task<T?> GetItemAsync<T>(string key)
    {
        var json = await GetItemAsync(key);
        if (string.IsNullOrEmpty(json))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize localStorage value for key '{Key}' as {Type}", key, typeof(T).Name);
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
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to serialize and store value for key '{Key}' as {Type}", key, typeof(T).Name);
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
