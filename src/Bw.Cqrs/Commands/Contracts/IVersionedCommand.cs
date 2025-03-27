using Bw.Cqrs.Command.Contract;

namespace Bw.Cqrs.Commands.Contracts;

public interface IVersionedCommand : ICommand
{
    int Version { get; }
} 