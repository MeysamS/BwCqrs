using Bw.Cqrs.Commands.Base;
using Bw.Cqrs.Commands.Enums;
using Bw.Cqrs.InternalCommands.Postgres.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bw.Cqrs.Commands.Postgres.Tests.Integration;

public class PostgresInternalCommandStoreTests : PostgresIntegrationTestBase
{
    private readonly PostgresInternalCommandStore _store;
    private readonly Mock<ILogger<PostgresInternalCommandStore>> _loggerMock;

    public PostgresInternalCommandStoreTests()
    {
        _loggerMock = new Mock<ILogger<PostgresInternalCommandStore>>();
        _store = new PostgresInternalCommandStore(DbContext, _loggerMock.Object);
    }

    [Fact]
    public async Task SaveAsync_ShouldSaveCommand()
    {
        // Arrange
        var command = new TestCommand { Data = "test" };

        // Act
        await _store.SaveAsync(command);

        // Assert
        var savedCommand = await DbContext.InternalCommands.FirstAsync();
        Assert.Equal(command.GetType().AssemblyQualifiedName, savedCommand.Type);
        Assert.Equal(command.Data, savedCommand.Data);
        Assert.Equal(InternalCommandStatus.Scheduled, savedCommand.Status);
    }

    [Fact]
    public async Task GetCommandsToExecuteAsync_ShouldReturnScheduledCommands()
    {
        // Arrange
        var command1 = new TestCommand { Data = "test1" };
        var command2 = new TestCommand { Data = "test2" };
        await _store.SaveAsync(command1);
        await _store.SaveAsync(command2);

        // Act
        var commands = await _store.GetCommandsToExecuteAsync();

        // Assert
        Assert.Equal(2, commands.Count());
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldUpdateCommandStatus()
    {
        // Arrange
        var command = new TestCommand { Data = "test" };
        await _store.SaveAsync(command);

        // Act
        await _store.UpdateStatusAsync(command.Id, InternalCommandStatus.Processing);

        // Assert
        var updatedCommand = await DbContext.InternalCommands.FirstAsync();
        Assert.Equal(InternalCommandStatus.Processing, updatedCommand.Status);
    }

    [Fact]
    public async Task CleanupAsync_ShouldDeleteOldCommands()
    {
        // Arrange
        var command = new TestCommand { Data = "test" };
        await _store.SaveAsync(command);
        await _store.UpdateStatusAsync(command.Id, InternalCommandStatus.Processed);

        // Act
        await _store.CleanupAsync(DateTime.UtcNow.AddDays(1));

        // Assert
        var commands = await DbContext.InternalCommands.ToListAsync();
        Assert.Empty(commands);
    }

    [Fact]
    public async Task GetStatsAsync_ShouldReturnCorrectStats()
    {
        // Arrange
        var command1 = new TestCommand { Data = "test1" };
        var command2 = new TestCommand { Data = "test2" };
        await _store.SaveAsync(command1);
        await _store.SaveAsync(command2);
        await _store.UpdateStatusAsync(command1.Id, InternalCommandStatus.Processed);

        // Act
        var stats = await _store.GetStatsAsync();

        // Assert
        Assert.Equal(2, stats.TotalCommands);
        Assert.Equal(1, stats.ScheduledCommands);
        Assert.Equal(1, stats.ProcessedCommands);
    }

    private class TestCommand : InternalCommand
    {
        public string Data { get; set; } = string.Empty;

        public TestCommand() : base()
        {
        }
    }
} 