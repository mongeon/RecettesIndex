using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using RecettesIndex;
using RecettesIndex.Services;
using Supabase;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();

// Securely load Supabase config from JSInterop (window.supabaseConfig)
var options = new SupabaseOptions
{
    AutoRefreshToken = true,
    AutoConnectRealtime = true,
    // SessionHandler = new SupabaseSessionHandler() <-- This must be implemented by the developer
};
var supabaseConfig = new SupabaseConfigDto
{
    url = builder.Configuration["supabase:Url"] ?? string.Empty,
    key = builder.Configuration["supabase:Key"] ?? string.Empty
};

builder.Services.AddSingleton(sp =>
    new Client(
        supabaseConfig.url,
        supabaseConfig.key,
        options
    )
);

builder.Services.AddScoped<AuthService>();



var host = builder.Build();


await host.RunAsync();

public class SupabaseConfigDto
{
    public string url { get; set; } = string.Empty;
    public string key { get; set; } = string.Empty;
}
