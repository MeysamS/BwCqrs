namespace Bw.Cqrs.Events.Contracts;

/// <summary>
/// Represents the event bus for publishing events to registered handlers
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes the specified event to all registered handlers
    /// </summary>
    /// <typeparam name="TEvent">The type of the event</typeparam>
    /// <param name="event">The event to publish</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) 
        where TEvent : IEvent;
    
    /// <summary>
    /// Publishes multiple events to their respective handlers
    /// </summary>
    /// <param name="events">The array of events to publish</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task PublishAsync(IEvent[] events, CancellationToken cancellationToken = default);
} 