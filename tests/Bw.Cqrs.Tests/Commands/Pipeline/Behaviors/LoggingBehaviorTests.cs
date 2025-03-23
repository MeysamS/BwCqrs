using Bw.Cqrs.Command;
using Bw.Cqrs.Commands.Base;
using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Commands.Pipeline.Behaviors;
using Bw.Cqrs.Common.Results;
using Microsoft.Extensions.Logging;
using Moq;

namespace Bw.Cqrs.Tests.Commands.Pipeline.Behaviors;

public class LoggingBehaviorTests
{
    private readonly Mock<ILogger<LoggingBehavior<TestCommand, IResult>>> _loggerMock;
    private readonly LoggingBehavior<TestCommand, IResult> _behavior;
    public LoggingBehaviorTests()
    {
        _loggerMock = new Mock<ILogger<LoggingBehavior<TestCommand, IResult>>>();
        _behavior = new LoggingBehavior<TestCommand, IResult>(_loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldLogCommandExecution()
    {
        // Arrange
        var command = new TestCommand();
        var expectedResult = CommandResult.Success();

        CommandHandlerDelegate<IResult> next = () => Task.FromResult<IResult>(expectedResult);


        // Act
        var result = await _behavior.HandleAsync(command, default, next);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Processing command started")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenExceptionOccurs_ShouldLogError()
    {
        // Arrange
        var command = new TestCommand();
        var expectedException = new Exception("Test exception");

        CommandHandlerDelegate<IResult> next = () => Task.FromException<IResult>(expectedException);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _behavior.HandleAsync(command, default, next));

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Processing command failed")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
    private class TestCommand : CommandBase
    {
    }
}