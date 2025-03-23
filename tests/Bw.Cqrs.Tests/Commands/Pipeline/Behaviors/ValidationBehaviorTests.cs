using Bw.Cqrs.Command;
using Bw.Cqrs.Commands.Base;
using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Commands.Pipeline.Behaviors;
using Bw.Cqrs.Common.Results;
using FluentAssertions;
using Moq;

namespace Bw.Cqrs.Tests.Commands.Pipeline.Behaviors;

public class ValidationBehaviorTests
{
    private readonly Mock<IValidationHandler<TestCommand>> _validationHandlerMock;
    private readonly ValidationBehavior<TestCommand, IResult> _behavior;

 public ValidationBehaviorTests()
    {
        _validationHandlerMock = new Mock<IValidationHandler<TestCommand>>();
        _behavior = new ValidationBehavior<TestCommand, IResult>(_validationHandlerMock.Object);
        _validationHandlerMock.Setup(x => x.ValidateAsync(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()));
    }
    
    [Fact]
    public async Task HandleAsync_WhenValidationSucceeds_ShouldCallNext()
    {
        // Arrange
        var command = new TestCommand();
        var exceptedResult = CommandResult.Success();
        var nextCalled = false;

        CommandHandlerDelegate<IResult> next = () =>
        {
            nextCalled = true;
            return Task.FromResult<IResult>(exceptedResult);
        };

        _validationHandlerMock.Setup(x => x.ValidateAsync(command, CancellationToken.None))
        .ReturnsAsync(CommandResult.Success());

        // Act
        var result = await _behavior.HandleAsync(command, default, next);

        // Assert
        result.Should().Be(exceptedResult);
        nextCalled.Should().BeTrue();
        _validationHandlerMock.Verify(x => x.ValidateAsync(command, CancellationToken.None), Times.Once);
    }


    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldNotCallNext()
    {
        // Arrange
        var command = new TestCommand();
        var validationError = CommandResult.Failure("Validation failed");
        var nextCalled = false;

        CommandHandlerDelegate<IResult> next = () =>
        {
            nextCalled = true;
            return Task.FromResult<IResult>(validationError);
        };

        _validationHandlerMock.Setup(x => x.ValidateAsync(command, CancellationToken.None))
        .ReturnsAsync(validationError);

        // Act
        var result = await _behavior.HandleAsync(command, default, next);

        // Assert
        result.Should().Be(validationError);
        nextCalled.Should().BeFalse();
        _validationHandlerMock.Verify(x => x.ValidateAsync(command, CancellationToken.None), Times.Once);
    }
    private class TestCommand : CommandBase
    {
    }

}