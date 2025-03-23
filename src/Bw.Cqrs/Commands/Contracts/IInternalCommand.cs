
using Bw.Cqrs.Command.Contract;

namespace Bw.Cqrs.Commands.Contracts;

public interface IInternalCommand : ICommand
{
    DateTime ScheduledOn { get; }
    DateTime? ProcessedOn { get; }
    string Type { get; }
    string Data { get; }
} 