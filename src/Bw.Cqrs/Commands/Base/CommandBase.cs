using Bw.Cqrs.Command.Contract;

namespace Bw.Cqrs.Commands.Base;

/// <summary>
/// Base class for all commands in the system
/// </summary>
public abstract class CommandBase : ICommand
{
    /// <summary>
    /// Gets the unique identifier for the command
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the timestamp when the command was created
    /// </summary>
    public DateTime Timestamp { get; private set; }

    /// <summary>
    /// Initializes a new instance of the command
    /// </summary>
    protected CommandBase()
    {
        Id = Guid.NewGuid();
        Timestamp = DateTime.UtcNow;
    }
} 