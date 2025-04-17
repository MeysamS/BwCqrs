using Bw.Cqrs.Events.Contracts;

namespace Bw.Cqrs.Events.Base;

/// <summary>
/// Base class for all events in the system
/// </summary>
public abstract class Event : IEvent
{
    /// <inheritdoc />
    public Guid Id { get; }

    /// <inheritdoc />
    public DateTime OccurredOn { get; }

    /// <summary>
    /// Initializes a new instance of the event with a new ID and current UTC timestamp
    /// </summary>
    protected Event()
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }
} 