using Bw.Cqrs.Commands.Base;
using Bw.Cqrs.Commands.Enums;
using Bw.Cqrs.InternalCommands.Postgres.Services;
using Bw.Cqrs.InternalCommands.Postgres.Data;
using Bw.Cqrs.InternalCommands.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bw.Cqrs.Commands.Postgres.Tests.Services;

public class PostgresInternalCommandStoreTests
{
    private readonly Mock<CommandDbContext> _dbContextMock;
    private readonly Mock<ILogger<PostgresInternalCommandStore>> _loggerMock;
    private readonly PostgresInternalCommandStore _store;

    public PostgresInternalCommandStoreTests()
    {
        _dbContextMock = new Mock<CommandDbContext>(new DbContextOptions<CommandDbContext>());
        _loggerMock = new Mock<ILogger<PostgresInternalCommandStore>>();
        _store = new PostgresInternalCommandStore(_dbContextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SaveAsync_ShouldSaveCommand()
    {
        // Arrange
        var command = new TestCommand { Data = "test" };
        var dbSetMock = new Mock<DbSet<CommandEntity>>();
        _dbContextMock.Setup(x => x.InternalCommands).Returns(dbSetMock.Object);

        // Act
        await _store.SaveAsync(command);

        // Assert
        dbSetMock.Verify(x => x.AddAsync(
            It.Is<CommandEntity>(e => 
                e.Id == command.Id &&
                e.Type == command.GetType().AssemblyQualifiedName &&
                e.Data.Contains("test") &&
                e.Status == InternalCommandStatus.Scheduled),
            It.IsAny<CancellationToken>()), 
            Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCommandsToExecuteAsync_ShouldReturnScheduledCommands()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var commands = new List<CommandEntity>
        {
            new() { Id = Guid.NewGuid(), Status = InternalCommandStatus.Scheduled, ScheduledOn = now.AddMinutes(-1) },
            new() { Id = Guid.NewGuid(), Status = InternalCommandStatus.Processing, ScheduledOn = now.AddMinutes(-1) },
            new() { Id = Guid.NewGuid(), Status = InternalCommandStatus.Scheduled, ScheduledOn = now.AddMinutes(1) }
        };

        var dbSetMock = new Mock<DbSet<CommandEntity>>();
        dbSetMock.As<IQueryable<CommandEntity>>().Setup(x => x.Provider).Returns(commands.AsQueryable().Provider);
        dbSetMock.As<IQueryable<CommandEntity>>().Setup(x => x.Expression).Returns(commands.AsQueryable().Expression);
        dbSetMock.As<IQueryable<CommandEntity>>().Setup(x => x.ElementType).Returns(commands.AsQueryable().ElementType);
        dbSetMock.As<IQueryable<CommandEntity>>().Setup(x => x.GetEnumerator()).Returns(commands.AsQueryable().GetEnumerator());

        _dbContextMock.Setup(x => x.InternalCommands).Returns(dbSetMock.Object);

        // Act
        var result = await _store.GetCommandsToExecuteAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(InternalCommandStatus.Scheduled, result.First().Status);
        Assert.True(result.First().ScheduledOn <= now);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldUpdateCommandStatus()
    {
        // Arrange
        var commandId = Guid.NewGuid();
        var command = new CommandEntity { Id = commandId, Status = InternalCommandStatus.Scheduled };
        var dbSetMock = new Mock<DbSet<CommandEntity>>();
        dbSetMock.Setup(x => x.FindAsync(new object[] { commandId }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(command);

        _dbContextMock.Setup(x => x.InternalCommands).Returns(dbSetMock.Object);

        // Act
        await _store.UpdateStatusAsync(commandId, InternalCommandStatus.Processing);

        // Assert
        Assert.Equal(InternalCommandStatus.Processing, command.Status);
        Assert.NotNull(command.LastRetryAt);
        Assert.Equal(1, command.RetryCount);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CleanupAsync_ShouldDeleteOldCommands()
    {
        // Arrange
        var cutoffDate = DateTime.UtcNow.AddDays(-1);
        var dbSetMock = new Mock<DbSet<CommandEntity>>();
        _dbContextMock.Setup(x => x.InternalCommands).Returns(dbSetMock.Object);

        // Act
        await _store.CleanupAsync(cutoffDate);

        // Assert
        dbSetMock.Verify(x => x.RemoveRange(
            It.Is<IEnumerable<CommandEntity>>(e => e.All(c => c.ProcessedOn < cutoffDate))),
            Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetStatsAsync_ShouldReturnCorrectStats()
    {
        // Arrange
        var commands = new List<CommandEntity>
        {
            new() { Status = InternalCommandStatus.Scheduled },
            new() { Status = InternalCommandStatus.Processing },
            new() { Status = InternalCommandStatus.Processed },
            new() { Status = InternalCommandStatus.Failed },
            new() { Status = InternalCommandStatus.Cancelled }
        };

        var dbSetMock = new Mock<DbSet<CommandEntity>>();
        dbSetMock.As<IQueryable<CommandEntity>>().Setup(x => x.Provider).Returns(commands.AsQueryable().Provider);
        dbSetMock.As<IQueryable<CommandEntity>>().Setup(x => x.Expression).Returns(commands.AsQueryable().Expression);
        dbSetMock.As<IQueryable<CommandEntity>>().Setup(x => x.ElementType).Returns(commands.AsQueryable().ElementType);
        dbSetMock.As<IQueryable<CommandEntity>>().Setup(x => x.GetEnumerator()).Returns(commands.AsQueryable().GetEnumerator());

        _dbContextMock.Setup(x => x.InternalCommands).Returns(dbSetMock.Object);

        // Act
        var stats = await _store.GetStatsAsync();

        // Assert
        Assert.Equal(5, stats.TotalCommands);
        Assert.Equal(1, stats.ScheduledCommands);
        Assert.Equal(1, stats.ProcessingCommands);
        Assert.Equal(1, stats.ProcessedCommands);
        Assert.Equal(1, stats.FailedCommands);
        Assert.Equal(1, stats.CancelledCommands);
    }

    private class TestCommand : InternalCommand
    {
        public string Data { get; set; } = string.Empty;

        public TestCommand() : base()
        {
        }
    }
}
