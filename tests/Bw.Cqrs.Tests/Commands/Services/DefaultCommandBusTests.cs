using Bw.Cqrs.Command.Contract;
using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Commands.Services;
using Bw.Cqrs.Common.Results;
using Moq;
using Microsoft.Extensions.DependencyInjection;
namespace Bw.Cqrs.Tests.Commands.Services;

using Bw.Cqrs.Command;
using Bw.Cqrs.Commands.Base;
using FluentAssertions;
using Xunit;

public class DefaultCommandBusTests
{
    private readonly Mock<ICommandHandlerFactory> _handlerFactoryMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IInternalCommandStore> _commandStoreMock;
    private readonly DefaultCommandBus _commandBus;

    public DefaultCommandBusTests()
    {
        _handlerFactoryMock = new Mock<ICommandHandlerFactory>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _commandStoreMock = new Mock<IInternalCommandStore>();
        _commandBus = new DefaultCommandBus(_handlerFactoryMock.Object, _commandStoreMock.Object, _serviceProviderMock.Object);
    }


    [Fact]
    public async Task DispatchAsync_ShouldExecuteCommandAndBehaviors()
    {
        // Arrange
        var command = new TestCommand();
        var handler = new Mock<ICommandHandler<TestCommand>>();
        var behavior = new Mock<ICommandPipelineBehavior<TestCommand, IResult>>();
        _handlerFactoryMock.Setup(x => x.Create<TestCommand>()).Returns(handler.Object);

        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IEnumerable<ICommandPipelineBehavior<TestCommand, IResult>>)))
            .Returns(new[] { behavior.Object });

        behavior
      .Setup(x => x.HandleAsync(command, default, It.IsAny<CommandHandlerDelegate<IResult>>()))
      .Returns((TestCommand cmd, CancellationToken token, CommandHandlerDelegate<IResult> next) => next());

        handler
      .Setup(x => x.HandleAsync(command, default))
      .ReturnsAsync(CommandResult.Success());

        // Act
        await _commandBus.DispatchAsync(command);

        // Assert
        handler.Verify(x => x.HandleAsync(command, default), Times.Once);
        behavior.Verify(x => x.HandleAsync(command, default, It.IsAny<CommandHandlerDelegate<IResult>>()), Times.Once);

    }

    private class TestCommand : CommandBase
    {
    }
}