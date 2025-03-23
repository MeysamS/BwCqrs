using System.Text.Json;
using Bw.Cqrs.Commands.Base;
using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Commands.Services;
using Bw.Cqrs.Postgres.Data;
using Bw.Cqrs.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bw.Cqrs.Postgres.Services;

public class PostgresInternalCommandStore : IInternalCommandStore
{
    private readonly CqrsDbContext _dbContext;
    private readonly ILogger<PostgresInternalCommandStore> _logger;

    public PostgresInternalCommandStore(
        CqrsDbContext dbContext,
        ILogger<PostgresInternalCommandStore> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SaveAsync(IInternalCommand command)
    {
        var entry = InternalCommandEntry.FromCommand(command);
        await _dbContext.InternalCommands.AddAsync(entry);
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation(
            "Saved internal command {CommandType} with ID {CommandId}",
            command.GetType().Name, ((CommandBase)command).Id);
    }

    public async Task<IEnumerable<IInternalCommand>> GetPendingCommandsAsync()
    {
        var entries = await _dbContext.InternalCommands
            .Where(x => x.ProcessedOn == null)
            .OrderBy(x => x.ScheduledOn)
            .ToListAsync();

        return entries.Select(entry =>
        {
            var type = Type.GetType(entry.Type);
            if (type == null)
            {
                _logger.LogError("Could not find type {Type}", entry.Type);
                return null;
            }

            var command = (IInternalCommand)JsonSerializer.Deserialize(entry.Data, type)!;
            return command;
        }).Where(x => x != null)!;
    }

    public async Task MarkAsProcessedAsync(Guid commandId)
    {
        var entry = await _dbContext.InternalCommands.FindAsync(commandId);
        if (entry != null)
        {
            entry.ProcessedOn = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation(
                "Marked internal command {CommandId} as processed",
                commandId);
        }
    }

    public async Task MarkAsFailedAsync(Guid commandId, Exception exception)
    {
        var entry = await _dbContext.InternalCommands.FindAsync(commandId);
        if (entry != null)
        {
            entry.Error = exception.ToString();
            await _dbContext.SaveChangesAsync();
            
            _logger.LogError(
                exception,
                "Marked internal command {CommandId} as failed",
                commandId);
        }
    }
} 