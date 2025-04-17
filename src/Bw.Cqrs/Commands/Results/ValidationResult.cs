namespace Bw.Cqrs.Commands.Results;

/// <summary>
/// Represents a validation result
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Initializes a new instance of the ValidationResult class
    /// </summary>
    /// <param name="message">The message</param>
    public ValidationResult(string message)
    {
        Message = message;
    }

    /// <summary>
    /// Gets the message
    /// </summary>
    public string Message { get; }
}
