namespace Bw.Cqrs.Common.Exceptions;

/// <summary>
/// Represents an exception thrown when a command handler is not found
/// </summary>
public class CommandHandlerNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the CommandHandlerNotFoundException class
    /// </summary>
    /// <param name="type">The type of command</param>
    public CommandHandlerNotFoundException(Type type)
        : base($"Command handler not found for command type: {type}")
    {
    }
}