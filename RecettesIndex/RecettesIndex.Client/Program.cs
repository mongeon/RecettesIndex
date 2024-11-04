using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RecettesIndex.Client.Data;
using Supabase;

namespace RecettesIndex.Client
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            ConfigureCommonServices(builder.Services, builder.Configuration);

            await builder.Build().RunAsync();
        }
        public static void ConfigureCommonServices(IServiceCollection services, IConfiguration configuration)
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
    }
}