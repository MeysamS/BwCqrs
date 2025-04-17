using Bw.Cqrs.Commands.Enums;
using Bw.Cqrs.InternalCommands.Postgres.Models;
using Xunit;

namespace Bw.Cqrs.Commands.Postgres.Tests.Models;

public class CommandEntityTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Act
        var command = new CommandEntity();

        // Assert
        Assert.Equal(Guid.Empty, command.Id);
        Assert.Null(command.Type);
        Assert.Null(command.Data);
        Assert.Equal(default, command.ScheduledOn);
        Assert.Equal(default, command.ProcessedOn);
        Assert.Equal(0, command.RetryCount);
        Assert.Null(command.Error);
        Assert.Equal(InternalCommandStatus.Scheduled, command.Status);
        Assert.Equal(DateTime.UtcNow.Date, command.CreatedAt.Date);
        Assert.Equal(default, command.LastRetryAt);
    }

    [Fact]
    public void ToString_ShouldReturnTypeAndId()
    {
        // Arrange
        var command = new CommandEntity
        {
            Id = Guid.NewGuid(),
            Type = "TestCommand"
        };

        // Act
        var result = command.ToString();

        // Assert
        Assert.Equal($"TestCommand ({command.Id})", result);
    }

    [Fact]
    public void ToString_WithNullType_ShouldReturnIdOnly()
    {
        // Arrange
        var command = new CommandEntity
        {
            Id = Guid.NewGuid()
        };

        // Act
        var result = command.ToString();

        // Assert
        Assert.Equal(command.Id.ToString(), result);
    }
} 