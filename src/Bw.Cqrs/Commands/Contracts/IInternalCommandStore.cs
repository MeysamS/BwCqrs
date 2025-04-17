
using Bw.Cqrs.Commands.Enums;
using Bw.Cqrs.Commands.Models;

namespace Bw.Cqrs.Commands.Contracts;

/// <summary>
/// Represents a store for internal commands
/// </summary>
public interface IInternalCommandStore
{
    /// <summary>
    /// Saves a command to the store
    /// </summary>
    Task SaveAsync(IInternalCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all commands that are ready to be executed
    /// </summary>
    Task<IEnumerable<IInternalCommand>> GetCommandsToExecuteAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the status of a command
    /// </summary>
    Task UpdateStatusAsync(Guid commandId, InternalCommandStatus status, string? error = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cleans up old processed commands
    /// </summary>
    Task CleanupAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets command statistics
    /// </summary>
    Task<InternalCommandStats> GetStatsAsync(CancellationToken cancellationToken = default);
} 