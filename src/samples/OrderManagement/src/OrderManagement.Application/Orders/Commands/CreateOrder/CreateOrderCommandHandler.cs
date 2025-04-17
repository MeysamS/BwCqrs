using Bw.Cqrs.Command;
using Bw.Cqrs.Command.Contract;
using Bw.Cqrs.Common.Results;
using Bw.Cqrs.Events.Contracts;
using OrderManagement.Domain.Events;
using OrderManagement.Domain.Models;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEventBus _eventBus;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IEventBus eventBus)
    {
        _orderRepository = orderRepository;
        _eventBus = eventBus;
    }

    public async Task<IResult> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken = default)
    {
        var items = command.Data.Items
            .Select(item => new OrderItem(
                item.ProductName,
                item.Quantity,
                item.UnitPrice))
            .ToList();

        var order = new Order(items);
        await _orderRepository.AddAsync(order);

        var @event = new OrderCreatedEvent(
            order.Id, 
            order.OrderNumber, 
            order.CreatedAt);
        await _eventBus.PublishAsync(@event);

        return CommandResult.Success();
    }
} 