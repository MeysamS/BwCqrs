using Bw.Cqrs.Commands.Enums;
using Bw.Cqrs.Commands.Postgres.Tests.Integration;
using Bw.Cqrs.InternalCommands.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Bw.Cqrs.Commands.Postgres.Tests.Data;

public class CommandDbContextTests : PostgresIntegrationTestBase
{
    [Fact]
    public async Task OnModelCreating_ShouldConfigureEntityCorrectly()
    {
        // Arrange
        var command = new CommandEntity
        {
            Type = "TestCommand",
            Data = "test data",
            Status = InternalCommandStatus.Scheduled,
            ScheduledOn = DateTime.UtcNow
        };

        // Act
        DbContext.InternalCommands.Add(command);
        await DbContext.SaveChangesAsync();

        // Assert
        var savedCommand = await DbContext.InternalCommands.FirstAsync();
        Assert.Equal("TestCommand", savedCommand.Type);
        Assert.Equal("test data", savedCommand.Data);
        Assert.Equal(InternalCommandStatus.Scheduled, savedCommand.Status);
        Assert.NotEqual(default, savedCommand.ScheduledOn);
    }

    [Fact]
    public async Task OnModelCreating_ShouldEnforceRequiredFields()
    {
        // Arrange
        var command = new CommandEntity
        {
            Status = InternalCommandStatus.Scheduled,
            ScheduledOn = DateTime.UtcNow
        };

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            DbContext.InternalCommands.Add(command);
            await DbContext.SaveChangesAsync();
        });
    }

    [Fact]
    public async Task OnModelCreating_ShouldEnforceMaxLength()
    {
        // Arrange
        var command = new CommandEntity
        {
            Type = new string('x', 256),
            Data = "test data",
            Status = InternalCommandStatus.Scheduled,
            ScheduledOn = DateTime.UtcNow
        };

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            DbContext.InternalCommands.Add(command);
            await DbContext.SaveChangesAsync();
        });
    }
} 