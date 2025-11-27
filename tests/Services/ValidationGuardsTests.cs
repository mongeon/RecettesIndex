using RecettesIndex.Services;
using Xunit;

namespace RecettesIndex.Tests.Services;

public class ValidationGuardsTests
{
    [Fact]
    public void RequireNotNull_Null_ReturnsMessage()
    {
        // Act
        var msg = ValidationGuards.RequireNotNull<string>(null, "Item");
        
        // Assert
        Assert.NotNull(msg);
        Assert.Contains("cannot be null", msg, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RequireNotNull_NotNull_ReturnsNull()
    {
        var msg = ValidationGuards.RequireNotNull("value", "Item");
        Assert.Null(msg);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RequireNonEmpty_Invalid_ReturnsMessage(string? value)
    {
        var msg = ValidationGuards.RequireNonEmpty(value, "Field");
        Assert.NotNull(msg);
        Assert.Contains("required", msg, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RequireNonEmpty_Valid_ReturnsNull()
    {
        var msg = ValidationGuards.RequireNonEmpty("abc", "Field");
        Assert.Null(msg);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RequirePositive_NonPositive_ReturnsMessage(int value)
    {
        var msg = ValidationGuards.RequirePositive(value, "id");
        Assert.NotNull(msg);
        Assert.Contains("invalid", msg, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RequirePositive_Positive_ReturnsNull()
    {
        var msg = ValidationGuards.RequirePositive(1, "id");
        Assert.Null(msg);
    }

    [Theory]
    [InlineData(-1, 0, 5)]
    [InlineData(6, 0, 5)]
    public void RequireInRange_OutOfRange_ReturnsMessage(int value, int min, int max)
    {
        var msg = ValidationGuards.RequireInRange(value, min, max, "rating");
        Assert.NotNull(msg);
        Assert.Contains("between", msg, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RequireInRange_InRange_ReturnsNull()
    {
        var msg = ValidationGuards.RequireInRange(3, 0, 5, "rating");
        Assert.Null(msg);
    }
}
