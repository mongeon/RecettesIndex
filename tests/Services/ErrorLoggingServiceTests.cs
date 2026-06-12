using Microsoft.Extensions.Logging;
using NSubstitute;
using RecettesIndex.Services;
using Supabase;
using Xunit;

namespace RecettesIndex.Tests.Services;

public class ErrorLoggingServiceTests
{
    private readonly Client _client;
    private readonly ILogger<ErrorLoggingService> _logger;
    private readonly ErrorLoggingService _service;

    public ErrorLoggingServiceTests()
    {
        _client = new Client("http://localhost", "test-key", new SupabaseOptions());
        _logger = Substitute.For<ILogger<ErrorLoggingService>>();
        _service = new ErrorLoggingService(_client, _logger);
    }

    [Fact]
    public async Task LogErrorAsync_SupabaseUnavailable_DoesNotThrow()
    {
        // Arrange
        var exception = new InvalidOperationException("Something went wrong");

        // Act - insert will fail (no Supabase connection) but must never bubble up
        var task = _service.LogErrorAsync(exception, "unit-test-context");

        // Assert
        var completed = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(30)));
        Assert.Same(task, completed);
        await task;
    }

    [Fact]
    public async Task LogErrorAsync_SupabaseUnavailable_LogsWarningFallback()
    {
        // Arrange
        var exception = new InvalidOperationException("Something went wrong");

        // Act
        await _service.LogErrorAsync(exception, "unit-test-context");

        // Assert - the persistence failure is reported through the local logger
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
