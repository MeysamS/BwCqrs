using Bw.Cqrs.Command.Base.Commands;

namespace OrderManagement.Application.Orders.Commands.CreateOrder;

public class CreateOrderRequest
{
    public List<OrderItemRequest> Items { get; set; } = new();
}

public class OrderItemRequest
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class CreateOrderCommand : CreateCommand<CreateOrderRequest>
{
    public CreateOrderCommand(CreateOrderRequest data) : base(data)
    {
    }
} 