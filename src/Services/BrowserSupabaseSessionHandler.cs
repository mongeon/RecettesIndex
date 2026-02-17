using System.Text.Json;
using Microsoft.JSInterop;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace RecettesIndex.Services;

/// <summary>
/// Persists Supabase auth sessions in browser localStorage for Blazor WebAssembly.
/// </summary>
public class BrowserSupabaseSessionHandler(IJSRuntime jsRuntime) : IGotrueSessionPersistence<Session>
{
    private const string SessionStorageKey = "recettes.supabase.auth.session";
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly IJSInProcessRuntime? _inProcessRuntime = jsRuntime as IJSInProcessRuntime;

    public void SaveSession(Session session)
    {
        if (_inProcessRuntime is null)
        {
            return;
        }

        try
        {
            var json = JsonSerializer.Serialize(session, SerializerOptions);
            _inProcessRuntime.InvokeVoid("localStorage.setItem", SessionStorageKey, json);
        }
        catch
        {
            // Ignore persistence errors to avoid breaking auth flow.
        }
    }

    public void DestroySession()
    {
        if (_inProcessRuntime is null)
        {
            return;
        }

        try
        {
            _inProcessRuntime.InvokeVoid("localStorage.removeItem", SessionStorageKey);
        }
        catch
        {
            // Ignore persistence errors to avoid breaking sign-out flow.
        }
    }

    public Session? LoadSession()
    {
        if (_inProcessRuntime is null)
        {
            return null;
        }

        try
        {
            var json = _inProcessRuntime.Invoke<string?>("localStorage.getItem", SessionStorageKey);
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<Session>(json, SerializerOptions);
        }
        catch
        {
            return null;
        }
    }
}