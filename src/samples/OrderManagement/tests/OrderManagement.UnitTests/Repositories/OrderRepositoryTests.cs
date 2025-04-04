using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using OrderManagement.Domain.Models;
using OrderManagement.Infrastructure.Persistence;
using OrderManagement.Infrastructure.Repositories;

namespace OrderManagement.UnitTests.Repositories;

public class OrderRepositoryTests
{
    private readonly Mock<IDbContext> _contextMock;
    private readonly Mock<DbSet<Order>> _ordersDbSetMock;
    private readonly Mock<IIncludableQueryable<Order, List<OrderItem>>> _includableQueryableMock;
    private readonly OrderRepository _repository;

    public OrderRepositoryTests()
    {
        _contextMock = new Mock<IDbContext>();
        _ordersDbSetMock = new Mock<DbSet<Order>>();
        _includableQueryableMock = new Mock<IIncludableQueryable<Order, List<OrderItem>>>();
        _contextMock.Setup(x => x.Orders).Returns(_ordersDbSetMock.Object);
        _repository = new OrderRepository(_contextMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrderExists_ReturnsOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order(new List<OrderItem>
        {
            new("Product 1", 2, 10)
        });

        _ordersDbSetMock.Setup(x => x.Include(o => o.Items))
            .Returns(_includableQueryableMock.Object);
        _includableQueryableMock.Setup(x => x.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _repository.GetByIdAsync(orderId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(order.Id, result.Id);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrderDoesNotExist_ReturnsNull()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _ordersDbSetMock.Setup(x => x.Include(o => o.Items))
            .Returns(_includableQueryableMock.Object);
        _includableQueryableMock.Setup(x => x.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _repository.GetByIdAsync(orderId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_WhenOrderIsValid_SavesOrder()
    {
        // Arrange
        var order = new Order(new List<OrderItem>
        {
            new("Product 1", 2, 10)
        });

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _repository.AddAsync(order);

        // Assert
        _ordersDbSetMock.Verify(x => x.AddAsync(order, It.IsAny<CancellationToken>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenOrderExists_UpdatesOrder()
    {
        // Arrange
        var order = new Order(new List<OrderItem>
        {
            new("Product 1", 2, 10)
        });

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _repository.UpdateAsync(order);

        // Assert
        _ordersDbSetMock.Verify(x => x.Update(order), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
} 