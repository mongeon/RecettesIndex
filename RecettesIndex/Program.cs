using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RecettesIndex;
using RecettesIndex.Client.Data;
using Supabase;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

ConfigureCommonServices(builder.Services, builder.Configuration);

await builder.Build().RunAsync();

static void ConfigureCommonServices(IServiceCollection services, IConfiguration configuration)
{
    var options = new SupabaseOptions
    {
        AutoRefreshToken = true,
        AutoConnectRealtime = true,
        // SessionHandler = new SupabaseSessionHandler() <-- This must be implemented by the developer
    };
    // Get supabase configuration from appsettings.json
    var supabaseConfig = configuration.GetSection("supabase").Get<SupabaseConfiguration>();
    services.AddSingleton(provider => new Supabase.Client(supabaseConfig?.Url ?? string.Empty, supabaseConfig?.Key ?? string.Empty, options));

    services.AddSingleton<IRecetteRepository, RecetteRepository>();
}