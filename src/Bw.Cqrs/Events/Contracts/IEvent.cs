namespace Bw.Cqrs.Events.Contracts;

/// <summary>
/// Represents the base contract for all events in the system
/// </summary>
public interface IEvent
{
    /// <summary>
    /// Gets the unique identifier of the event
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the UTC date and time when the event occurred
    /// </summary>
    DateTime OccurredOn { get; }
} 