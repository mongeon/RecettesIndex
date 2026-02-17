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

// Securely load Supabase config from JSInterop (window.supabaseConfig)
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
builder.Services.AddScoped<LocalStorageService>();

var host = builder.Build();


await host.RunAsync();

public class SupabaseConfigDto
{
    public string Url { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
}
