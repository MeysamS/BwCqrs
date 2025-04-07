namespace Bw.Cqrs.Commands.Contracts;

public interface IVersionedCommand : ICommand
{
    int Version { get; }
} 