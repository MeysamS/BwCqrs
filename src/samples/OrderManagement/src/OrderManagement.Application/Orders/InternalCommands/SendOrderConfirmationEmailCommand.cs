using Bw.Cqrs.Commands;
using Bw.Cqrs.Commands.Base;

namespace OrderManagement.Application.Orders.InternalCommands;

public class SendOrderConfirmationEmailCommand : InternalCommand
{
    public Guid OrderId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
} 