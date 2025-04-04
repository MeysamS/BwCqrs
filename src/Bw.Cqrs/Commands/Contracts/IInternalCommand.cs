using Bw.Cqrs.Command.Contract;
using Bw.Cqrs.Commands.Enums;

namespace Bw.Cqrs.Commands.Contracts;

/// <summary>
/// Represents a command that can be scheduled for later execution
/// </summary>
public interface IInternalCommand : ICommand
{
    /// <summary>
    /// Gets the date and time when the command was scheduled
    /// </summary>
    DateTime ScheduledOn { get; }

    /// <summary>
    /// Gets the date and time when the command should be executed
    /// </summary>
    DateTime ExecuteAt { get; }

    /// <summary>
    /// Gets the date and time when the command was processed (if it has been processed)
    /// </summary>
    DateTime? ProcessedOn { get; }

    /// <summary>
    /// Gets the number of processing attempts made
    /// </summary>
    int RetryCount { get; }

    /// <summary>
    /// Gets the error message if the command failed
    /// </summary>
    string? Error { get; }

    /// <summary>
    /// Gets the status of the command
    /// </summary>
    InternalCommandStatus Status { get; }
} 