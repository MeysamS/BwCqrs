namespace Bw.Cqrs.Events.Contracts;

/// <summary>
/// Represents a handler for processing events of type <typeparamref name="TEvent"/>
/// </summary>
/// <typeparam name="TEvent">The type of event to handle</typeparam>
public interface IEventHandler<in TEvent>
    where TEvent : IEvent
{
    /// <summary>
    /// Handles the specified event asynchronously
    /// </summary>
    /// <param name="event">The event to handle</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
} 