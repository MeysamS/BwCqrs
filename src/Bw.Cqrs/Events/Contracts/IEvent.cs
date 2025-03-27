namespace Bw.Cqrs.Events.Contracts;

public interface IEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
} 