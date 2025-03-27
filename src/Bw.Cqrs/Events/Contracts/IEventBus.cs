namespace Bw.Cqrs.Events.Contracts;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) 
        where TEvent : IEvent;
    
    Task PublishAsync(IEvent[] events, CancellationToken cancellationToken = default);
} 