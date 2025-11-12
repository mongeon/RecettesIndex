namespace RecettesIndex.Services.Exceptions;

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Gets the list of validation errors.
    /// </summary>
    public List<string> Errors { get; init; } = new();

    /// <summary>
    /// Initializes a new instance of the ValidationException class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the validation error.</param>
    public ValidationException(string message) : base(message) 
    {
        Errors.Add(message);
    }

    /// <summary>
    /// Initializes a new instance of the ValidationException class with a list of validation errors.
    /// </summary>
    /// <param name="errors">The list of validation error messages.</param>
    public ValidationException(List<string> errors) : base("Validation failed")
    {
        Errors = errors ?? new List<string>();
    }
}
