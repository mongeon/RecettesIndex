using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RecettesIndex.Api.Data;
using Supabase;

var options = new SupabaseOptions
{
    AutoRefreshToken = true,
    AutoConnectRealtime = true,
    // SessionHandler = new SupabaseSessionHandler() <-- This must be implemented by the developer
};

var builder = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddSingleton(provider => new Supabase.Client(Environment.GetEnvironmentVariable("Supabase.Url") ?? string.Empty, Environment.GetEnvironmentVariable("Supabase.Key") ?? string.Empty, options));
        services.AddSingleton<IRecetteRepository, RecetteRepository>();
    });


var host = builder.Build();

host.Run();