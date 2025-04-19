using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Queries.Contracts;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Orders.Commands.CreateOrder;
using OrderManagement.Application.Orders.Commands.UpdateOrderStatus;
using OrderManagement.Application.Orders.Queries.GetOrders;

namespace OrderManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(ICommandProcessor commandProcessor,IQueryProcessor queryProcessor) : ControllerBase
{

    
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var command = new CreateOrderCommand(request);
        await commandProcessor.DispatchAsync(command);
        return Ok();
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(
        Guid id, 
        [FromBody] UpdateOrderStatusRequest request)
    {
        var command = new UpdateOrderStatusCommand(id, request);
        await commandProcessor.DispatchAsync(command);
        return Ok();
    }


    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var query = new GetOrderQuery();
        var result = await queryProcessor.SendAsync(query);
        return Ok(result);
    }
} 