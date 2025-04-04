using Bw.Cqrs.Events.Base;
using OrderManagement.Domain.Models;
namespace OrderManagement.Domain.Events;

public class OrderStatusChangedEvent : Event
{
    public Guid OrderId { get; }
    public OrderStatus OldStatus { get; }
    public OrderStatus NewStatus { get; }

    public OrderStatusChangedEvent(Guid orderId, OrderStatus oldStatus, OrderStatus newStatus)
    {
        OrderId = orderId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
} 