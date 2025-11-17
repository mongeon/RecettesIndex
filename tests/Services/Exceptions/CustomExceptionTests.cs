using RecettesIndex.Services.Exceptions;

namespace RecettesIndex.Tests.Services.Exceptions;

/// <summary>
/// Tests for custom exception classes used in the service layer.
/// </summary>
public class CustomExceptionTests
{
    [Fact]
    public void NotFoundException_WithEntityNameAndId_CreatesCorrectMessage()
    {
        // Arrange
        var entityName = "Recipe";
        var entityId = 123;

        // Act
        var exception = new NotFoundException(entityName, entityId);

        // Assert
        Assert.Contains(entityName, exception.Message);
        Assert.Contains(entityId.ToString(), exception.Message);
        Assert.Contains("not found", exception.Message);
        Assert.IsType<NotFoundException>(exception);
        Assert.Equal(entityName, exception.EntityType);
        Assert.Equal(entityId, exception.EntityId);
    }

    [Theory]
    [InlineData("Book", 1)]
    [InlineData("Author", 999)]
    [InlineData("Recipe", 42)]
    public void NotFoundException_VariousEntities_CreatesExpectedMessage(string entityName, int id)
    {
        // Arrange & Act
        var exception = new NotFoundException(entityName, id);

        // Assert
        Assert.Contains(entityName, exception.Message);
        Assert.Contains(id.ToString(), exception.Message);
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public void ServiceException_WithMessage_SetsMessageCorrectly()
    {
        // Arrange
        var expectedMessage = "A service error occurred";

        // Act
        var exception = new ServiceException(expectedMessage);

        // Assert
        Assert.Equal(expectedMessage, exception.Message);
        Assert.IsType<ServiceException>(exception);
    }

    [Fact]
    public void ServiceException_WithInnerException_PreservesInnerException()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");
        var message = "Service error";

        // Act
        var exception = new ServiceException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void ValidationException_WithMessage_SetsMessageCorrectly()
    {
        // Arrange
        var expectedMessage = "Validation failed for the entity";

        // Act
        var exception = new ValidationException(expectedMessage);

        // Assert
        Assert.Equal(expectedMessage, exception.Message);
        Assert.IsType<ValidationException>(exception);
        Assert.Single(exception.Errors);
        Assert.Contains(expectedMessage, exception.Errors);
    }

    [Theory]
    [InlineData("Email is required")]
    [InlineData("Rating must be between 1 and 5")]
    [InlineData("Name cannot be empty")]
    public void ValidationException_VariousMessages_StoresCorrectly(string message)
    {
        // Arrange & Act
        var exception = new ValidationException(message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Contains(message, exception.Errors);
    }

    [Fact]
    public void ValidationException_WithErrorList_StoresAllErrors()
    {
        // Arrange
        var errors = new List<string>
        {
            "Error 1",
            "Error 2",
            "Error 3"
        };

        // Act
        var exception = new ValidationException(errors);

        // Assert
        Assert.Equal("Validation failed", exception.Message);
        Assert.Equal(3, exception.Errors.Count);
        Assert.All(errors, error => Assert.Contains(error, exception.Errors));
    }

    [Fact]
    public void ExceptionHierarchy_ServiceExceptionIsBaseException()
    {
        // Arrange & Act
        var notFoundEx = new NotFoundException("Entity", 1);
        var validationEx = new ValidationException("Validation error");
        var serviceEx = new ServiceException("Service error");

        // Assert
        Assert.IsType<NotFoundException>(notFoundEx);
        Assert.IsType<ValidationException>(validationEx);
        Assert.IsAssignableFrom<Exception>(serviceEx);
        Assert.IsAssignableFrom<Exception>(notFoundEx);
        Assert.IsAssignableFrom<Exception>(validationEx);
    }

    [Fact]
    public void NotFoundException_WithStringId_CreatesCorrectMessage()
    {
        // Arrange
        var entityType = "User";
        var entityId = "abc-123";

        // Act
        var exception = new NotFoundException(entityType, entityId);

        // Assert
        Assert.Contains(entityType, exception.Message);
        Assert.Contains(entityId, exception.Message);
        Assert.Equal(entityType, exception.EntityType);
        Assert.Equal(entityId, exception.EntityId);
    }

    [Fact]
    public void NotFoundException_WithCustomMessage_UsesProvidedMessage()
    {
        // Arrange
        var customMessage = "Custom not found message";

        // Act
        var exception = new NotFoundException(customMessage);

        // Assert
        Assert.Equal(customMessage, exception.Message);
        Assert.Equal("Entity", exception.EntityType);
    }
}
