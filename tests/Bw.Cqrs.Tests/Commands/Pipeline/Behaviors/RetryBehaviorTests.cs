using Bw.Cqrs.Command;
using Bw.Cqrs.Command.Base.Commands;
using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Commands.Pipeline.Behaviors;
using Bw.Cqrs.Common.Results;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Bw.Cqrs.Tests.Commands.Pipeline.Behaviors;

public class RetryBehaviorTests
{
    private readonly Mock<ILogger<RetryBehavior<TestCommand, IResult>>> _loggerMock;
    private readonly RetryBehavior<TestCommand, IResult> _behavior;

    public RetryBehaviorTests()
    {
        _loggerMock = new Mock<ILogger<RetryBehavior<TestCommand, IResult>>>();
        _behavior = new RetryBehavior<TestCommand, IResult>(_loggerMock.Object, maxRetries: 2, delayMilliseconds: 100);
    }

    [Fact]
    public async Task HandleAsync_WhenNoException_ShouldSucceed()
    {
        // Arrange
        var command = new TestCommand();
        var expectedResult = CommandResult.Success();
        CommandHandlerDelegate<IResult> next = () => Task.FromResult<IResult>(expectedResult);

        // Act
        var result = await _behavior.HandleAsync(command, default, next);

        // Assert
        result.Should().Be(expectedResult);
        _loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenRetryableException_ShouldRetryAndSucceed()
    {
        // Arrange
        var command = new TestCommand();
        var expectedResult = CommandResult.Success();
        var attemptCount = 0;
        
        CommandHandlerDelegate<IResult> next = () =>
        {
            attemptCount++;
            if (attemptCount == 1)
            {
                throw new TimeoutException("Timeout");
            }
            return Task.FromResult<IResult>(expectedResult);
        };

        // Act
        var result = await _behavior.HandleAsync(command, default, next);

        // Assert
        result.Should().Be(expectedResult);
        attemptCount.Should().Be(2);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retry attempt")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenMaxRetriesExceeded_ShouldThrowCommandRetryException()
    {
        // Arrange
        var command = new TestCommand();
        CommandHandlerDelegate<IResult> next = () => throw new TimeoutException("Timeout");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CommandRetryException>(
            () => _behavior.HandleAsync(command, default, next));

        exception.Message.Should().Contain("failed after");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retry attempt")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task HandleAsync_WhenNonRetryableException_ShouldNotRetry()
    {
        // Arrange
        var command = new TestCommand();
        var exception = new ArgumentException("Invalid argument");
        CommandHandlerDelegate<IResult> next = () => throw exception;

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<ArgumentException>(
            () => _behavior.HandleAsync(command, default, next));

        thrownException.Should().Be(exception);
        _loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    private class TestCommand : CreateCommand<TestRequest>
    {
        public TestCommand() : base(new TestRequest())
        {
        }
    }

    private class TestRequest
    {
        public string Data { get; set; } = string.Empty;
    }
} 