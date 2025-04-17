using Bw.Cqrs.Command.Base.Commands;
using Bw.Cqrs.Commands;
using OrderManagement.Domain.Models;

namespace OrderManagement.Application.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusRequest
{
    public OrderStatus NewStatus { get; set; }
}

public class UpdateOrderStatusCommand : UpdateCommand<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusCommand(Guid orderId, UpdateOrderStatusRequest data) 
        : base(orderId, data)
    {
    }
} 