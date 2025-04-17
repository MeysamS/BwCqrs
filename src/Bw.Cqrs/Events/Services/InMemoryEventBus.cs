using Bw.Cqrs.Events.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bw.Cqrs.Events.Services;

/// <summary>
/// In-memory implementation of the event bus that dispatches events to registered handlers
/// </summary>
public class InMemoryEventBus : IEventBus
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InMemoryEventBus> _logger;

    /// <summary>
    /// Initializes a new instance of the InMemoryEventBus
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving event handlers</param>
    /// <param name="logger">The logger instance</param>
    public InMemoryEventBus(
        IServiceProvider serviceProvider,
        ILogger<InMemoryEventBus> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) 
        where TEvent : IEvent
    {
        try
        {
            _logger.LogDebug("Publishing event {EventType} with ID {EventId}", 
                typeof(TEvent).Name, @event.Id);

            var handlers = _serviceProvider.GetServices<IEventHandler<TEvent>>();

            foreach (var handler in handlers)
            {
                try
                {
                    await handler.HandleAsync(@event, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling event {EventType} by handler {HandlerType}", 
                        typeof(TEvent).Name, handler.GetType().Name);
                    // We continue with other handlers even if one fails
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event {EventType}", typeof(TEvent).Name);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task PublishAsync(IEvent[] events, CancellationToken cancellationToken = default)
    {
        foreach (var @event in events)
        {
            // Use reflection to call the generic PublishAsync method
            var method = typeof(IEventBus)
                .GetMethod(nameof(PublishAsync), new[] { @event.GetType(), typeof(CancellationToken) });

            if (method == null)
            {
                throw new InvalidOperationException($"Could not find PublishAsync method for event type {@event.GetType()}");
            }

            await (Task)method.Invoke(this, new object[] { @event, cancellationToken })!;
        }
    }
} 