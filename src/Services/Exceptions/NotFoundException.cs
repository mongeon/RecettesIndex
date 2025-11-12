namespace RecettesIndex.Services.Exceptions;

/// <summary>
/// Exception thrown when a requested entity is not found.
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Gets the type of the entity that was not found.
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// Gets the identifier of the entity that was not found.
    /// </summary>
    public object? EntityId { get; }

    /// <summary>
    /// Initializes a new instance of the NotFoundException class with a specified entity type and identifier.
    /// </summary>
    /// <param name="entityType">The type of the entity that was not found.</param>
    /// <param name="id">The identifier of the entity that was not found.</param>
    public NotFoundException(string entityType, int id)
        : base($"{entityType} with ID {id} was not found")
    {
        EntityType = entityType;
        EntityId = id;
    }

    /// <summary>
    /// Initializes a new instance of the NotFoundException class with a specified entity type and identifier.
    /// </summary>
    /// <param name="entityType">The type of the entity that was not found.</param>
    /// <param name="id">The identifier of the entity that was not found.</param>
    public NotFoundException(string entityType, string id)
        : base($"{entityType} with ID '{id}' was not found")
    {
        EntityType = entityType;
        EntityId = id;
    }

    /// <summary>
    /// Initializes a new instance of the NotFoundException class with a custom message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public NotFoundException(string message) : base(message)
    {
        EntityType = "Entity";
    }
}
