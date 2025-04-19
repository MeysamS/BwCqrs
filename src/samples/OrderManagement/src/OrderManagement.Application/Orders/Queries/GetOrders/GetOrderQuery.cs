using Bw.Cqrs.Queries.Contracts;
using OrderManagement.Domain.Models;

namespace OrderManagement.Application.Orders.Queries.GetOrders;



public class OrdersResponse
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = default!;
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public record GetOrderQuery : IQuery<IEnumerable<OrdersResponse>>
{
}
