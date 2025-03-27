using Bw.Cqrs.Events.Contracts;

namespace Bw.Cqrs.Events.Base;

public abstract class Event : IEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }

    protected Event()
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }
} 