using Bw.Cqrs.Command;
using Bw.Cqrs.Command.Contract;
using Bw.Cqrs.Commands.Base;
using Bw.Cqrs.Commands.Enums;
using Bw.Cqrs.Common.Results;

namespace OrderManagement.Application.Orders.InternalCommands;

public class SendOrderConfirmationEmailCommand : InternalCommand
{

    public Guid OrderId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}

public class SendOrderConfirmationEmailCommandHandler : ICommandHandler<SendOrderConfirmationEmailCommand>
{
    public Task<IResult> HandleAsync(SendOrderConfirmationEmailCommand command, CancellationToken cancellationToken = default)
    {
        return Task.FromResult((IResult)new CommandResult(true,"Email sent successfully"));    
    }
}