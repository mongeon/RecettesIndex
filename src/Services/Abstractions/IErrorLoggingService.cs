namespace RecettesIndex.Services.Abstractions;

public interface IErrorLoggingService
{
    Task LogErrorAsync(Exception ex, string context);
}
