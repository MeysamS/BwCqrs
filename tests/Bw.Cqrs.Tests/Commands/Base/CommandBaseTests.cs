using Bw.Cqrs.Commands.Base;
using FluentAssertions;

namespace Bw.Cqrs.Tests.Commands.Base;

public class CommandBaseTests
{
    [Fact]
    public void Constructor_ShouldInitialize_IdAndTimestamp()
    {
                // Arrange & Act

        var command = new TestCommand();

        // Assert
        command.Id.Should().NotBe(Guid.Empty);
        command.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    
    }

    private class TestCommand : CommandBase
    {
    }
}