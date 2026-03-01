using Microsoft.Extensions.Logging;
using RecettesIndex.Models;
using RecettesIndex.Services.Abstractions;

namespace RecettesIndex.Services;

public class ErrorLoggingService(Supabase.Client supabaseClient, ILogger<ErrorLoggingService> logger) : IErrorLoggingService
{
    public async Task LogErrorAsync(Exception ex, string context)
    {
        try
        {
            var log = new AppLog
            {
                Level = "Error",
                Message = ex.Message,
                Context = context,
                StackTrace = ex.StackTrace
            };

            await supabaseClient.From<AppLog>().Insert(log);
        }
        catch (Exception loggingEx)
        {
            // Never let the logging service itself crash the app
            logger.LogWarning(loggingEx, "Failed to persist error log to Supabase");
        }
    }
}
