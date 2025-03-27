using Bw.Cqrs.Command;
using Bw.Cqrs.Command.Base.Commands;
using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Commands.Pipeline.Behaviors;
using Bw.Cqrs.Common.Results;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Bw.Cqrs.Tests.Commands.Pipeline.Behaviors;

public class ErrorHandlingBehaviorTests
{
    private readonly Mock<ILogger<ErrorHandlingBehavior<TestCommand, IResult>>> _loggerMock;
    private readonly ErrorHandlingBehavior<TestCommand, IResult> _behavior;

    public ErrorHandlingBehaviorTests()
    {
        _loggerMock = new Mock<ILogger<ErrorHandlingBehavior<TestCommand, IResult>>>();
        _behavior = new ErrorHandlingBehavior<TestCommand, IResult>(_loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenNoException_ShouldReturnResult()
    {
        // Arrange
        var command = new TestCommand();
        var expectedResult = CommandResult.Success();
        CommandHandlerDelegate<IResult> next = () => Task.FromResult<IResult>(expectedResult);

        // Act
        var result = await _behavior.HandleAsync(command, default, next);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task HandleAsync_WhenValidationException_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new TestCommand();
        var validationException = new ValidationException("Validation failed");
        CommandHandlerDelegate<IResult> next = () => Task.FromException<IResult>(validationException);

        // Act
        var result = await _behavior.HandleAsync(command, default, next);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Validation failed");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Validation failed")),
                validationException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenUnexpectedException_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new TestCommand();
        var exception = new Exception("Unexpected error");
        CommandHandlerDelegate<IResult> next = () => Task.FromException<IResult>(exception);

        // Act
        var result = await _behavior.HandleAsync(command, default, next);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("An unexpected error occurred while processing the command");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error processing command")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
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