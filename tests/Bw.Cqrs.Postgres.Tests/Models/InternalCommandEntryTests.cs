using Bw.Cqrs.Commands.Base;
using Bw.Cqrs.Postgres.Models;
using FluentAssertions;

namespace Bw.Cqrs.Postgres.Tests.Models;

public class InternalCommandEntryTests
{
    [Fact]
    public void FromCommand_ShouldCreateValidEntry()
    {
        // Arrange
        var command = new TestCommand
        {
            TestProperty = "Test Value"
        };

        // Act
        var entry = InternalCommandEntry.FromCommand(command);

        // Assert
        entry.Should().NotBeNull();
        entry.Id.Should().Be(command.Id);
        entry.Type.Should().Be(typeof(TestCommand).FullName);
        entry.ScheduledOn.Should().Be(command.ScheduledOn);
        entry.ProcessedOn.Should().BeNull();
        entry.Error.Should().BeNull();
        entry.Data.Should().Contain("Test Value");
    }

    private class TestCommand : InternalCommandBase
    {
        public string TestProperty { get; set; } = default!;
    }
} 