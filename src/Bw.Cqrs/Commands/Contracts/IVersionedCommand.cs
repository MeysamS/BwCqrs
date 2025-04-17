using Bw.Cqrs.Command.Contract;

namespace Bw.Cqrs.Commands.Contracts;

/// <summary>
/// Represents a command that supports versioning
/// </summary>
public interface IVersionedCommand : ICommand
{
    /// <summary>
    /// Gets the version of the command
    /// </summary>
    int Version { get; }
} 