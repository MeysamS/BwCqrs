using System.Text.Json;
using Bw.Cqrs.Commands.Base;
using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Commands.Enums;
using Bw.Cqrs.Commands.Models;
using Bw.Cqrs.InternalCommands.Postgres.Models;
using Bw.Cqrs.InternalCommands.Postgres.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bw.Cqrs.InternalCommands.Postgres.Services;

public class PostgresInternalCommandStore : IInternalCommandStore
{
    private readonly CommandDbContext _dbContext;
    private readonly ILogger<PostgresInternalCommandStore> _logger;

    public PostgresInternalCommandStore(
        CommandDbContext dbContext,
        ILogger<PostgresInternalCommandStore> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SaveAsync(IInternalCommand command, CancellationToken cancellationToken = default)
    {
        var entity = new CommandEntity
        {
            Id = ((CommandBase)command).Id,
            Type = command.GetType().AssemblyQualifiedName!,
            Data = JsonSerializer.Serialize(command),
            ScheduledOn = command.ScheduledOn,
            Status = command.Status,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.InternalCommands.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Command {CommandId} saved to PostgreSQL", entity.Id);
    }

    public async Task<IEnumerable<IInternalCommand>> GetCommandsToExecuteAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var commands = await _dbContext.InternalCommands
            .Where(x => x.Status == InternalCommandStatus.Scheduled && x.ScheduledOn <= now)
            .ToListAsync(cancellationToken);

        return commands.Select(x => DeserializeCommand(x));
    }

    public async Task UpdateStatusAsync(Guid commandId, InternalCommandStatus status, string? error = null, CancellationToken cancellationToken = default)
    {
        var command = await _dbContext.InternalCommands.FindAsync(new object[] { commandId }, cancellationToken);
        if (command == null)
        {
            _logger.LogWarning("Command {CommandId} not found for status update", commandId);
            return;
        }

        command.Status = status;
        command.Error = error;

        if (status == InternalCommandStatus.Processing)
        {
            command.LastRetryAt = DateTime.UtcNow;
            command.RetryCount++;
        }
        else if (status == InternalCommandStatus.Processed)
        {
            command.ProcessedOn = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("Command {CommandId} status updated to {Status}", commandId, status);
    }

    public async Task CleanupAsync(DateTime cutoffDate, CancellationToken cancellationToken = default)
    {
        var deletedCount = await _dbContext.InternalCommands
            .Where(x => x.ProcessedOn < cutoffDate)
            .ExecuteDeleteAsync(cancellationToken);

        _logger.LogInformation("Cleaned up {Count} old commands", deletedCount);
    }

    public async Task<InternalCommandStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var stats = new InternalCommandStats
        {
            TotalCommands = await _dbContext.InternalCommands.CountAsync(cancellationToken),
            ScheduledCommands = await _dbContext.InternalCommands.CountAsync(x => x.Status == InternalCommandStatus.Scheduled, cancellationToken),
            ProcessingCommands = await _dbContext.InternalCommands.CountAsync(x => x.Status == InternalCommandStatus.Processing, cancellationToken),
            ProcessedCommands = await _dbContext.InternalCommands.CountAsync(x => x.Status == InternalCommandStatus.Processed, cancellationToken),
            FailedCommands = await _dbContext.InternalCommands.CountAsync(x => x.Status == InternalCommandStatus.Failed, cancellationToken),
            CancelledCommands = await _dbContext.InternalCommands.CountAsync(x => x.Status == InternalCommandStatus.Cancelled, cancellationToken)
        };

        return stats;
    }

    private static IInternalCommand DeserializeCommand(CommandEntity entity)
    {
        var type = Type.GetType(entity.Type);
        if (type == null)
            throw new InvalidOperationException($"Type {entity.Type} not found");

        var command = (IInternalCommand)JsonSerializer.Deserialize(entity.Data, type)!;
        return command;
    }
} 