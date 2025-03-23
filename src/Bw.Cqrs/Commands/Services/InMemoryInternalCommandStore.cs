using System.Collections.Concurrent;
using Bw.Cqrs.Commands.Base;
using Bw.Cqrs.Commands.Contracts;

namespace Bw.Cqrs.Commands.Services;

public class InMemoryInternalCommandStore : IInternalCommandStore
{
    private readonly ConcurrentDictionary<Guid, IInternalCommand> _commands = new();

    public Task SaveAsync(IInternalCommand command)
    {
        _commands.TryAdd(((CommandBase)command).Id, command);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<IInternalCommand>> GetPendingCommandsAsync()
    {
        var pendingCommands = _commands.Values
            .Where(x => x.ProcessedOn == null)
            .ToList();
        return Task.FromResult<IEnumerable<IInternalCommand>>(pendingCommands);
    }

    public Task MarkAsProcessedAsync(Guid commandId)
    {
        if (_commands.TryGetValue(commandId, out var command))
        {
            ((InternalCommandBase)command).MarkAsProcessed();
        }
        return Task.CompletedTask;
    }
} 