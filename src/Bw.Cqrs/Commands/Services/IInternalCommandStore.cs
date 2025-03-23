using Bw.Cqrs.Commands.Contracts;

namespace Bw.Cqrs.Commands.Services;

public interface IInternalCommandStore
{
    Task SaveAsync(IInternalCommand command);
    Task<IEnumerable<IInternalCommand>> GetPendingCommandsAsync();
    Task MarkAsProcessedAsync(Guid commandId);
} 