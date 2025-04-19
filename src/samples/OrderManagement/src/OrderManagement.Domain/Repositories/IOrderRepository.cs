using System;
using System.Threading.Tasks;
using OrderManagement.Domain.Models;

namespace OrderManagement.Domain.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task<IEnumerable<Order>> GetAllAsync();
} 