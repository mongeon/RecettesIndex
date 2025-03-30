using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using RecettesIndex;
using RecettesIndex.Api.Data;
using RecettesIndex.Data;
using RecettesIndex.Services;
using Supabase;

var options = new SupabaseOptions
{
    AutoRefreshToken = true,
    AutoConnectRealtime = true,
    // SessionHandler = new SupabaseSessionHandler() <-- This must be implemented by the developer
};
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["API_Prefix"] ?? builder.HostEnvironment.BaseAddress) });
var supabaseConfig = new SupabaseConfiguration
{
    Url = builder.Configuration["supabase:Url"] ?? string.Empty,
    Key = builder.Configuration["supabase:Key"] ?? string.Empty
};



builder.Services.AddSingleton(provider => new Supabase.Client(supabaseConfig.Url ?? string.Empty, supabaseConfig.Key ?? string.Empty, options));
builder.Services.AddScoped<IRecetteRepository, RecetteRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<AuthorService>();
builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<RecetteService>();

await builder.Build().RunAsync();
