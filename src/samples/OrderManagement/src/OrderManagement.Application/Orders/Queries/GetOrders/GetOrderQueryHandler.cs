using Bw.Cqrs.Queries.Contracts;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Application.Orders.Queries.GetOrders;

public class GetOrderQueryHandler(IOrderRepository orderRepository) : IQueryHandler<GetOrderQuery, IEnumerable<OrdersResponse>>
{
    public async Task<IEnumerable<OrdersResponse>> HandleAsync(GetOrderQuery query)
    {
        var orders = await orderRepository.GetAllAsync();
        var result = new List<OrdersResponse>();

        foreach (var item in orders)
        {
            result.Add(new OrdersResponse
            {
                Id = item.Id,
                OrderNumber = item.OrderNumber,
                Status = item.Status,
                CreatedAt = item.CreatedAt
            });
        }
        return result;

    }
}