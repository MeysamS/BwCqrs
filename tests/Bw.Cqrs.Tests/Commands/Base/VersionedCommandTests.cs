using Bw.Cqrs.Command.Base.Commands;
using Bw.Cqrs.Commands.Contracts;
using FluentAssertions;

namespace Bw.Cqrs.Tests.Commands.Base;

public class VersionedCommandTests
{
    [Fact]
    public void VersionedCommand_ShouldImplementIVersionedCommand()
    {
        // Arrange & Act
        var command = new TestVersionedCommand(new TestRequest(), 2);

        // Assert
        command.Should().BeAssignableTo<IVersionedCommand>();
        command.Version.Should().Be(2);
    }

    [Fact]
    public void VersionedCommand_WhenNotSpecified_ShouldDefaultToVersion1()
    {
        // Arrange & Act
        var command = new TestVersionedCommand(new TestRequest());

        // Assert
        command.Version.Should().Be(1);
    }

    private class TestVersionedCommand : CreateCommand<TestRequest>, IVersionedCommand
    {
        public int Version { get; }

        public TestVersionedCommand(TestRequest data, int version = 1) : base(data)
        {
            Version = version;
        }
    }

    private class TestRequest
    {
        public string Data { get; set; } = string.Empty;
    }
} 