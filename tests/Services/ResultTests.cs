using RecettesIndex.Services;
using Xunit;

namespace RecettesIndex.Tests.Services;

/// <summary>
/// Unit tests for Result<T> pattern
/// </summary>
public class ResultTests
{
    #region Success Tests

    [Fact]
    public void Success_WithValue_CreatesSuccessResult()
    {
        // Arrange
        var expectedValue = "test value";

        // Act
        var result = Result<string>.Success(expectedValue);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedValue, result.Value);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Success_WithNullValue_CreatesSuccessResultWithNull()
    {
        // Act
        var result = Result<string?>.Success(null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Success_WithIntValue_CreatesSuccessResult()
    {
        // Arrange
        var expectedValue = 42;

        // Act
        var result = Result<int>.Success(expectedValue);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedValue, result.Value);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Success_WithComplexObject_CreatesSuccessResult()
    {
        // Arrange
        var expectedValue = new { Id = 1, Name = "Test" };

        // Act
        var result = Result<dynamic>.Success(expectedValue);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedValue, result.Value);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Success_WithList_CreatesSuccessResult()
    {
        // Arrange
        var expectedValue = new List<int> { 1, 2, 3 };

        // Act
        var result = Result<List<int>>.Success(expectedValue);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedValue, result.Value);
        Assert.Null(result.ErrorMessage);
    }

    #endregion

    #region Failure Tests

    [Fact]
    public void Failure_WithErrorMessage_CreatesFailureResult()
    {
        // Arrange
        var expectedError = "Operation failed";

        // Act
        var result = Result<string>.Failure(expectedError);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedError, result.ErrorMessage);
    }

    [Fact]
    public void Failure_WithEmptyMessage_CreatesFailureResult()
    {
        // Act
        var result = Result<string>.Failure(string.Empty);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(string.Empty, result.ErrorMessage);
    }

    [Fact]
    public void Failure_WithNullMessage_CreatesFailureResult()
    {
        // Act
        var result = Result<string>.Failure(null!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Failure_ForIntType_CreatesFailureResultWithDefaultValue()
    {
        // Arrange
        var expectedError = "Integer operation failed";

        // Act
        var result = Result<int>.Failure(expectedError);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(0, result.Value); // Default value for int
        Assert.Equal(expectedError, result.ErrorMessage);
    }

    [Fact]
    public void Failure_ForReferenceType_CreatesFailureResultWithNull()
    {
        // Arrange
        var expectedError = "Reference type operation failed";

        // Act
        var result = Result<object>.Failure(expectedError);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedError, result.ErrorMessage);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void IsSuccess_SuccessResult_ReturnsTrue()
    {
        // Arrange
        var result = Result<string>.Success("test");

        // Act & Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void IsSuccess_FailureResult_ReturnsFalse()
    {
        // Arrange
        var result = Result<string>.Failure("error");

        // Act & Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Value_SuccessResult_ReturnsExpectedValue()
    {
        // Arrange
        var expectedValue = "success value";
        var result = Result<string>.Success(expectedValue);

        // Act & Assert
        Assert.Equal(expectedValue, result.Value);
    }

    [Fact]
    public void Value_FailureResult_ReturnsDefault()
    {
        // Arrange
        var result = Result<string>.Failure("error");

        // Act & Assert
        Assert.Null(result.Value);
    }

    [Fact]
    public void ErrorMessage_SuccessResult_ReturnsNull()
    {
        // Arrange
        var result = Result<string>.Success("test");

        // Act & Assert
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ErrorMessage_FailureResult_ReturnsExpectedMessage()
    {
        // Arrange
        var expectedError = "error message";
        var result = Result<string>.Failure(expectedError);

        // Act & Assert
        Assert.Equal(expectedError, result.ErrorMessage);
    }

    #endregion

    #region Edge Cases

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("a very long error message that exceeds normal length expectations")]
    public void Failure_VariousErrorMessages_StoresCorrectly(string errorMessage)
    {
        // Act
        var result = Result<string>.Failure(errorMessage);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(errorMessage, result.ErrorMessage);
    }

    [Fact]
    public void Success_WithZero_CreatesSuccessResult()
    {
        // Act
        var result = Result<int>.Success(0);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Success_WithFalse_CreatesSuccessResult()
    {
        // Act
        var result = Result<bool>.Success(false);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Success_WithEmptyString_CreatesSuccessResult()
    {
        // Act
        var result = Result<string>.Success(string.Empty);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(string.Empty, result.Value);
        Assert.Null(result.ErrorMessage);
    }

    #endregion

    #region Multiple Result Types

    [Fact]
    public void DifferentResultTypes_CanCoexist()
    {
        // Arrange & Act
        var stringResult = Result<string>.Success("test");
        var intResult = Result<int>.Success(42);
        var boolResult = Result<bool>.Failure("failed");

        // Assert
        Assert.True(stringResult.IsSuccess);
        Assert.True(intResult.IsSuccess);
        Assert.False(boolResult.IsSuccess);
    }

    #endregion
}
