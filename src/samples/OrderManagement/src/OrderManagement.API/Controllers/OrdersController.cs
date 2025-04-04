using System;
using System.Threading.Tasks;
using Bw.Cqrs.Command.Contract;
using Bw.Cqrs.Commands;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Orders.Commands.CreateOrder;
using OrderManagement.Application.Orders.Commands.UpdateOrderStatus;

namespace OrderManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ICommandBus _commandBus;

    public OrdersController(ICommandBus commandBus)
    {
        _commandBus = commandBus;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var command = new CreateOrderCommand(request);
        await _commandBus.DispatchAsync(command);
        return Ok();
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(
        Guid id, 
        [FromBody] UpdateOrderStatusRequest request)
    {
        var command = new UpdateOrderStatusCommand(id, request);
        await _commandBus.DispatchAsync(command);
        return Ok();
    }
} 