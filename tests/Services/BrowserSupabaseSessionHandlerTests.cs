using Microsoft.JSInterop;
using NSubstitute;
using RecettesIndex.Services;
using Supabase.Gotrue;
using System.Text.Json;

namespace RecettesIndex.Tests.Services;

public class BrowserSupabaseSessionHandlerTests
{
    [Fact]
    public void SaveSession_WithInProcessRuntime_WritesToLocalStorage()
    {
        var jsRuntime = Substitute.For<IJSInProcessRuntime>();
        var handler = new BrowserSupabaseSessionHandler(jsRuntime);
        var session = new Session
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token"
        };

        handler.SaveSession(session);

        jsRuntime.Received(1).InvokeVoid("localStorage.setItem", Arg.Any<object?[]>());
    }

    [Fact]
    public void LoadSession_WithSerializedSession_ReturnsSession()
    {
        var jsRuntime = Substitute.For<IJSInProcessRuntime>();
        var handler = new BrowserSupabaseSessionHandler(jsRuntime);
        var expectedSession = new Session
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token"
        };
        var serialized = JsonSerializer.Serialize(expectedSession, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        jsRuntime.Invoke<string?>("localStorage.getItem", Arg.Any<object?[]>()).Returns(serialized);

        var result = handler.LoadSession();

        Assert.NotNull(result);
        Assert.Equal("access-token", result!.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
    }

    [Fact]
    public void DestroySession_WithInProcessRuntime_RemovesFromLocalStorage()
    {
        var jsRuntime = Substitute.For<IJSInProcessRuntime>();
        var handler = new BrowserSupabaseSessionHandler(jsRuntime);

        handler.DestroySession();

        jsRuntime.Received(1).InvokeVoid("localStorage.removeItem", Arg.Any<object?[]>());
    }
}