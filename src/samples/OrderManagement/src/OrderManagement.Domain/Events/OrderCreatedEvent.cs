
using Bw.Cqrs.Events.Base;

namespace OrderManagement.Domain.Events;

public class OrderCreatedEvent :Event
{
    public Guid OrderId { get; }
    public string OrderNumber { get; }
    public DateTime CreatedAt { get; }

    public OrderCreatedEvent(Guid orderId, string orderNumber, DateTime createdAt)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        CreatedAt = createdAt;
    }
} 