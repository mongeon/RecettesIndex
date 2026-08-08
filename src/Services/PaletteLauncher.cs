namespace RecettesIndex.Services;

/// <summary>
/// Lets any page open the ⌘K palette without holding a reference to it.
/// </summary>
/// <remarks>
/// The palette lives in the layout, because it has to be reachable from everywhere and
/// there must only ever be one. A page cannot hold a <c>@ref</c> across that boundary, so
/// the request travels as an event instead: the layout subscribes once, pages raise it.
/// This is what the home page's search field is — a very large button that opens the
/// palette, rather than a second search implementation.
/// </remarks>
public sealed class PaletteLauncher
{
    /// <summary>Raised when a page asks for the palette. The layout is the only listener.</summary>
    public event Func<Task>? Requested;

    /// <summary>
    /// Opens the palette, or does nothing when no layout is listening — a page must not
    /// fail because it was rendered outside the usual chrome.
    /// </summary>
    public Task OpenAsync() => Requested?.Invoke() ?? Task.CompletedTask;
}
