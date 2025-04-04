using Bw.Cqrs.Events.Base;
using OrderManagement.Domain.Events;

namespace OrderManagement.Domain.Models;

public class Order
{
    private readonly List<Event> _domainEvents = new();
    public IReadOnlyCollection<Event> DomainEvents => _domainEvents.AsReadOnly();

    public Guid Id { get; private set; }
    public string OrderNumber { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public List<OrderItem> Items { get; private set; }

    private Order()
    {
        Items = new List<OrderItem>();
        CreatedAt = DateTime.UtcNow;
        Status = OrderStatus.Created;
    }

    public Order(List<OrderItem> items) : this()
    {
        Id = Guid.NewGuid();
        OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Id.ToString().Substring(0, 8)}";
        Items = items;
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        if (Status == newStatus)
            return;

        var oldStatus = Status;
        Status = newStatus;

        // Add domain event
        var @event = new OrderStatusChangedEvent(Id, oldStatus, newStatus);
        _domainEvents.Add(@event);
    }

    public decimal CalculateTotal()
    {
        return Items.Sum(item => item.Quantity * item.UnitPrice);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

public enum OrderStatus
{
    Created,
    Processing,
    Shipped,
    Delivered,
    Cancelled
} 