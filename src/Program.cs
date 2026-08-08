using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using MudBlazor.Services;
using RecettesIndex;
using RecettesIndex.Services;
using RecettesIndex.Services.Abstractions;
using Supabase;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();

// Load Supabase config from appsettings.json. In Blazor WASM all client-side
// config is public; this is the anon key, which is safe to expose (RLS enforces access).
var supabaseConfig = new SupabaseConfigDto
{
    Url = builder.Configuration["supabase:Url"] ?? string.Empty,
    Key = builder.Configuration["supabase:Key"] ?? string.Empty
};

builder.Services.AddSingleton(sp =>
{
    var options = new SupabaseOptions
    {
        AutoRefreshToken = true,
        AutoConnectRealtime = false,
        SessionHandler = new BrowserSupabaseSessionHandler(sp.GetRequiredService<IJSRuntime>())
    };

    return new Client(
        supabaseConfig.Url,
        supabaseConfig.Key,
        options
    );
});

builder.Services.AddScoped<ISupabaseAuthWrapper, SupabaseAuthWrapper>();
builder.Services.AddScoped<RecettesIndex.Services.AuthService>();
builder.Services.AddScoped<IBookAuthorService, BookAuthorService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IRecipesQuery, SupabaseRecipesQuery>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IStoreService, StoreService>();
builder.Services.AddScoped<IEtiquetteService, EtiquetteService>();
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddScoped<IErrorLoggingService, ErrorLoggingService>();
// Le lien entre les pages et la palette ⌘K, qui vit dans la mise en page.
builder.Services.AddScoped<PaletteLauncher>();

var host = builder.Build();


await host.RunAsync();

public class SupabaseConfigDto
{
    public string Url { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
}
