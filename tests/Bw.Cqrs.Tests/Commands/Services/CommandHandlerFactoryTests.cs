using Bw.Cqrs.Command.Contract;
using Bw.Cqrs.Commands.Base;
using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Commands.Services;
using Bw.Cqrs.Common.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bw.Cqrs.Tests.Commands.Services;

public class CommandHandlerFactoryTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<CommandHandlerFactory>> _loggerMock;
    private readonly CommandHandlerFactory _factory;

    public CommandHandlerFactoryTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _loggerMock = new Mock<ILogger<CommandHandlerFactory>>();
        _factory = new CommandHandlerFactory(_serviceProviderMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void Create_WithCommand_ShouldReturnHandler()
    {
        // Arrange
        var handler = new Mock<ICommandHandler<TestCommand>>();
        _serviceProviderMock
            .Setup(x => x.GetService(typeof(ICommandHandler<TestCommand>)))
            .Returns(handler.Object);

        // Act
        var result = _factory.Create<TestCommand>();

        // Assert
        Assert.Same(handler.Object, result);
    }

    [Fact]
    public void Create_WithCommandAndResult_ShouldReturnHandler()
    {
        // Arrange
        var handler = new Mock<ICommandHandler<TestCommand, TestResult>>();
        _serviceProviderMock
            .Setup(x => x.GetService(typeof(ICommandHandler<TestCommand, TestResult>)))
            .Returns(handler.Object);

        // Act
        var result = _factory.Create<TestCommand, TestResult>();

        // Assert
        Assert.Same(handler.Object, result);
    }

    private class TestCommand : CommandBase
    {
    }

    private class TestResult : IResult
    {
        public bool IsSuccess => true;
        public string? ErrorMessage => null;
    }
}