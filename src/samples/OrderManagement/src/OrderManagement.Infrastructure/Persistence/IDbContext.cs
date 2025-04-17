using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Models;

namespace OrderManagement.Infrastructure.Persistence;

public interface IDbContext 
{
    DbSet<Order> Orders { get; set; }
    DbSet<OrderItem> OrderItems { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
} 