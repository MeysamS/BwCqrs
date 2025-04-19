using Bw.Cqrs.Commands.Contracts;
using Newtonsoft.Json;

namespace Bw.Cqrs.Commands.Base;

/// <summary>
/// Base class for all commands in the system
/// </summary>
public abstract class CommandBase : ICommand
{
    /// <summary>
    /// Gets the unique identifier for the command
    /// </summary>
    [JsonProperty]
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the timestamp when the command was created
    /// </summary>
    [JsonProperty]
    public DateTime Timestamp { get; private set; }

    /// <summary>
    /// Initializes a new instance of the command
    /// </summary>
    public CommandBase()
    {
        Id = Guid.NewGuid();
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the command
    /// </summary>
    /// <param name="id"></param>
    /// <param name="timestamp"></param>
    [JsonConstructor]
    public CommandBase(Guid id, DateTime timestamp)
    {
        Id = id;
        Timestamp = timestamp;
    }
}