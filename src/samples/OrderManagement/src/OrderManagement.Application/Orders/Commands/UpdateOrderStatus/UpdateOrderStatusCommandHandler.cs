using Bw.Cqrs.Command;
using Bw.Cqrs.Command.Contract;
using Bw.Cqrs.Common.Results;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Application.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandHandler : ICommandHandler<UpdateOrderStatusCommand>
{
    private readonly IOrderRepository _orderRepository;

    public UpdateOrderStatusCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IResult> HandleAsync(UpdateOrderStatusCommand command, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(command.EntityId);
        if (order == null)
            return CommandResult.Failure("Order not found");

        order.UpdateStatus(command.Data.NewStatus);
        await _orderRepository.UpdateAsync(order);

        return CommandResult.Success();
    }
} 