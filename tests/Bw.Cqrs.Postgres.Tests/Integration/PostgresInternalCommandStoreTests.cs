using Bw.Cqrs.Commands.Base;
using Bw.Cqrs.Postgres.Data;
using Bw.Cqrs.Postgres.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Testcontainers.PostgreSql;
using Xunit;

namespace Bw.Cqrs.Postgres.Tests.Integration;

public class PostgresInternalCommandStoreTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    private DbContextOptions<CqrsDbContext>? _dbContextOptions;
    private readonly Mock<ILogger<PostgresInternalCommandStore>> _loggerMock;

    public PostgresInternalCommandStoreTests()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            .WithDatabase("test_db")
            .WithUsername("test_user")
            .WithPassword("test_pass")
            .Build();

        _loggerMock = new Mock<ILogger<PostgresInternalCommandStore>>();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        
        _dbContextOptions = new DbContextOptionsBuilder<CqrsDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        // Create database
        await using var context = new CqrsDbContext(_dbContextOptions);
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    private CqrsDbContext CreateContext()
    {
        if (_dbContextOptions == null) throw new InvalidOperationException("DbContext options not initialized");
        return new CqrsDbContext(_dbContextOptions);
    }

    [Fact]
    public async Task SaveAsync_ShouldPersistCommand()
    {
        // Arrange
        await using var context = CreateContext();
        var store = new PostgresInternalCommandStore(context, _loggerMock.Object);
        var command = new TestCommand { TestProperty = "Test" };

        // Act
        await store.SaveAsync(command);

        // Assert
        var entry = await context.InternalCommands.FindAsync(command.Id);
        entry.Should().NotBeNull();
        entry!.Type.Should().Be(typeof(TestCommand).FullName);
        entry.Data.Should().Contain("Test");
    }

    [Fact]
    public async Task GetPendingCommandsAsync_ShouldReturnUnprocessedCommands()
    {
        // Arrange
        await using var context = CreateContext();
        var store = new PostgresInternalCommandStore(context, _loggerMock.Object);
        
        var command1 = new TestCommand { TestProperty = "Test1" };
        var command2 = new TestCommand { TestProperty = "Test2" };
        
        await store.SaveAsync(command1);
        await store.SaveAsync(command2);
        await store.MarkAsProcessedAsync(command1.Id);

        // Act
        var pendingCommands = await store.GetPendingCommandsAsync();

        // Assert
        pendingCommands.Should().HaveCount(1);
        var pendingCommand = pendingCommands.First() as TestCommand;
        pendingCommand.Should().NotBeNull();
        pendingCommand!.TestProperty.Should().Be("Test2");
    }

    [Fact]
    public async Task MarkAsProcessedAsync_ShouldUpdateProcessedOn()
    {
        // Arrange
        await using var context = CreateContext();
        var store = new PostgresInternalCommandStore(context, _loggerMock.Object);
        var command = new TestCommand { TestProperty = "Test" };
        await store.SaveAsync(command);

        // Act
        await store.MarkAsProcessedAsync(command.Id);

        // Assert
        var entry = await context.InternalCommands.FindAsync(command.Id);
        entry.Should().NotBeNull();
        entry!.ProcessedOn.Should().NotBeNull();
    }

    [Fact]
    public async Task MarkAsFailedAsync_ShouldUpdateError()
    {
        // Arrange
        await using var context = CreateContext();
        var store = new PostgresInternalCommandStore(context, _loggerMock.Object);
        var command = new TestCommand { TestProperty = "Test" };
        await store.SaveAsync(command);
        var exception = new Exception("Test error");

        // Act
        await store.MarkAsFailedAsync(command.Id, exception);

        // Assert
        var entry = await context.InternalCommands.FindAsync(command.Id);
        entry.Should().NotBeNull();
        entry!.Error.Should().Contain("Test error");
    }

    private class TestCommand : InternalCommandBase
    {
        public string TestProperty { get; set; } = default!;
    }
} 