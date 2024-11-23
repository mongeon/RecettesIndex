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
        var b = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process);
        var supabaseConfig = new SupabaseConfiguration
        {
            Url = Environment.GetEnvironmentVariable("Supabase.Url", EnvironmentVariableTarget.Process) ?? string.Empty,
            Key = Environment.GetEnvironmentVariable("Supabase.Key", EnvironmentVariableTarget.Process) ?? string.Empty
        };

        services.AddSingleton(provider => new Supabase.Client(supabaseConfig.Url ?? string.Empty, supabaseConfig.Key ?? string.Empty, options));
        services.AddSingleton<IRecetteRepository, RecetteRepository>();
    });


var host = builder.Build();

host.Run();