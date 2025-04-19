using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Events.Contracts;

using Microsoft.Extensions.Logging;

using OrderManagement.Application.Orders.InternalCommands;
using OrderManagement.Domain.Events;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Application.Orders.EventHandlers;

public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
{
    private readonly ICommandProcessor _commandProcessor;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(
        ICommandProcessor commandBus,
        IOrderRepository orderRepository,
        ILogger<OrderCreatedEventHandler> logger)
    {
        _commandProcessor = commandBus;
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task HandleAsync(OrderCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(@event.OrderId);
        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found when processing OrderCreatedEvent", @event.OrderId);
            return;
        }

        // Schedule confirmation email
        var command = new SendOrderConfirmationEmailCommand
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            TotalAmount = order.CalculateTotal(),
            // In a real application, you would get the customer email from the order
            CustomerEmail = "customer@example.com"
        };
        await _commandProcessor.ScheduleAsync(command);

        _logger.LogInformation(
            "Order {OrderNumber} created and confirmation email scheduled",
            @event.OrderNumber);
    }
}