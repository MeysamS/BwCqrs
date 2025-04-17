using System.Collections.Concurrent;
using Bw.Cqrs.Commands.Base;
using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Commands.Enums;
using Bw.Cqrs.Commands.Models;
using Microsoft.Extensions.Logging;

namespace Bw.Cqrs.Commands.Services;

/// <summary>
/// In-memory implementation of internal command store
/// </summary>
public class InMemoryInternalCommandStore : IInternalCommandStore
{
    private readonly ConcurrentDictionary<Guid, IInternalCommand> _commands = new();
    private readonly ILogger<InMemoryInternalCommandStore> _logger;

    /// <summary>
    /// Initializes a new instance of the InMemoryInternalCommandStore class
    /// </summary>
    /// <param name="logger">The logger</param>
    public InMemoryInternalCommandStore(ILogger<InMemoryInternalCommandStore> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Saves a command
    /// </summary>
    /// <param name="command">The command</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task</returns>
    public Task SaveAsync(IInternalCommand command, CancellationToken cancellationToken = default)
    {
        if (_commands.TryAdd(((CommandBase)command).Id, command))
        {
            _logger.LogDebug("Command {CommandId} saved successfully", ((CommandBase)command).Id);
            return Task.CompletedTask;
        }

        throw new InvalidOperationException($"Command with ID {((CommandBase)command).Id} already exists");
    }

    /// <summary>
    /// Gets commands to execute
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task</returns>
    public Task<IEnumerable<IInternalCommand>> GetCommandsToExecuteAsync(CancellationToken cancellationToken = default)
    {
        var commands = _commands.Values
            .Where(x => x is InternalCommand ic && ic.IsReadyToExecute())
            .ToList();

        _logger.LogDebug("Found {Count} commands ready to execute", commands.Count);
        return Task.FromResult<IEnumerable<IInternalCommand>>(commands);
    }

    /// <summary>
    /// Updates the status of a command
    /// </summary>
    /// <param name="commandId">The command ID</param>
    /// <param name="status">The status</param>
    /// <param name="error">The error</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public Task UpdateStatusAsync(Guid commandId, InternalCommandStatus status, string? error = null, CancellationToken cancellationToken = default)
    {
        if (_commands.TryGetValue(commandId, out var command) && command is InternalCommand ic)
        {
            switch (status)
            {
                case InternalCommandStatus.Processing:
                    ic.MarkAsProcessing();
                    break;
                case InternalCommandStatus.Processed:
                    ic.MarkAsProcessed();
                    break;
                case InternalCommandStatus.Failed:
                    ic.MarkAsFailed(error ?? "Unknown error");
                    break;
                case InternalCommandStatus.Cancelled:
                    ic.MarkAsCancelled();
                    break;
            }

            _logger.LogDebug("Command {CommandId} status updated to {Status}", commandId, status);
            return Task.CompletedTask;
        }

        throw new InvalidOperationException($"Command with ID {commandId} not found");
    }

    /// <summary>
    /// Cleans up old commands
    /// </summary>
    /// <param name="cutoffDate">The cutoff date</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task</returns>
    public Task CleanupAsync(DateTime cutoffDate, CancellationToken cancellationToken = default)
    {
        var oldCommands = _commands.Values
            .Where(x => x.ProcessedOn.HasValue && x.ProcessedOn.Value < cutoffDate)
            .Select(x => ((CommandBase)x).Id)
            .ToList();

        foreach (var id in oldCommands)
        {
            _commands.TryRemove(id, out _);
        }

        _logger.LogInformation("Cleaned up {Count} old commands", oldCommands.Count);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the stats of the internal command store
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task</returns>
    public Task<InternalCommandStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var stats = new InternalCommandStats
        {
            TotalCommands = _commands.Count,
            ScheduledCommands = _commands.Values.Count(x => x.Status == InternalCommandStatus.Scheduled),
            ProcessingCommands = _commands.Values.Count(x => x.Status == InternalCommandStatus.Processing),
            ProcessedCommands = _commands.Values.Count(x => x.Status == InternalCommandStatus.Processed),
            FailedCommands = _commands.Values.Count(x => x.Status == InternalCommandStatus.Failed),
            CancelledCommands = _commands.Values.Count(x => x.Status == InternalCommandStatus.Cancelled)
        };

        return Task.FromResult(stats);
    }
    
} 