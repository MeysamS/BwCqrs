using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Models;
using OrderManagement.Infrastructure.Persistence;
using OrderManagement.Infrastructure.Repositories;
using Testcontainers.PostgreSql;
using Xunit;

namespace OrderManagement.IntegrationTests.Repositories;

public class OrderRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    private readonly ApplicationDbContext _context;
    private readonly OrderRepository _repository;

    public OrderRepositoryIntegrationTests()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            .WithPassword("postgres")
            .WithDatabase("order_management_test")
            .Build();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new OrderRepository(_context);
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _container.DisposeAsync();
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrderExists_ReturnsOrderWithItems()
    {
        // Arrange
        var order = new Order(new List<OrderItem>
        {
            new("Product 1", 2, 10),
            new("Product 2", 1, 20)
        });
        await _repository.AddAsync(order);

        // Act
        var result = await _repository.GetByIdAsync(order.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(order.OrderNumber, result.OrderNumber);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(40, result.CalculateTotal());
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrderDoesNotExist_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_WhenOrderIsValid_SavesOrderToDatabase()
    {
        // Arrange
        var order = new Order(new List<OrderItem>
        {
            new("Product 1", 2, 10)
        });

        // Act
        await _repository.AddAsync(order);

        // Assert
        var savedOrder = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == order.Id);

        Assert.NotNull(savedOrder);
        Assert.Equal(order.OrderNumber, savedOrder.OrderNumber);
        Assert.Single(savedOrder.Items);
        Assert.Equal("Product 1", savedOrder.Items[0].ProductName);
    }

    [Fact]
    public async Task UpdateAsync_WhenOrderExists_UpdatesOrderInDatabase()
    {
        // Arrange
        var order = new Order(new List<OrderItem>
        {
            new("Product 1", 2, 10)
        });
        await _repository.AddAsync(order);

        // Act
        order.UpdateStatus(OrderStatus.Processing);
        await _repository.UpdateAsync(order);

        // Assert
        var updatedOrder = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == order.Id);

        Assert.NotNull(updatedOrder);
        Assert.Equal(OrderStatus.Processing, updatedOrder.Status);
    }
} 