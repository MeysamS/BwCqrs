using Bw.Cqrs.Events.Base;
using Bw.Cqrs.Events.Contracts;
using Bw.Cqrs.Events.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Bw.Cqrs.Tests.Events.Services;

public class InMemoryEventBusTests
{
    private readonly Mock<ILogger<InMemoryEventBus>> _loggerMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly InMemoryEventBus _eventBus;

    public InMemoryEventBusTests()
    {
        _loggerMock = new Mock<ILogger<InMemoryEventBus>>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _eventBus = new InMemoryEventBus(_serviceProviderMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task PublishAsync_WhenNoHandlers_ShouldNotThrowException()
    {
        // Arrange
        var @event = new TestEvent();
        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IEnumerable<IEventHandler<TestEvent>>)))
            .Returns(Array.Empty<IEventHandler<TestEvent>>());

        // Act
        await _eventBus.PublishAsync(@event);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Publishing event")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WhenHandlersExist_ShouldCallAllHandlers()
    {
        // Arrange
        var @event = new TestEvent { Data = "Test" };
        var handler1 = new Mock<IEventHandler<TestEvent>>();
        var handler2 = new Mock<IEventHandler<TestEvent>>();

        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IEnumerable<IEventHandler<TestEvent>>)))
            .Returns(new[] { handler1.Object, handler2.Object });

        // Act
        await _eventBus.PublishAsync(@event);

        // Assert
        handler1.Verify(x => x.HandleAsync(@event, It.IsAny<CancellationToken>()), Times.Once);
        handler2.Verify(x => x.HandleAsync(@event, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WhenHandlerThrows_ShouldContinueWithOtherHandlers()
    {
        // Arrange
        var @event = new TestEvent();
        var handler1 = new Mock<IEventHandler<TestEvent>>();
        var handler2 = new Mock<IEventHandler<TestEvent>>();

        handler1
            .Setup(x => x.HandleAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Handler 1 failed"));

        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IEnumerable<IEventHandler<TestEvent>>)))
            .Returns(new[] { handler1.Object, handler2.Object });

        // Act
        await _eventBus.PublishAsync(@event);

        // Assert
        handler2.Verify(x => x.HandleAsync(@event, It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error handling event")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WithMultipleEvents_ShouldPublishAllEvents()
    {
        // Arrange
        var events = new IEvent[]
        {
            new TestEvent { Data = "Event 1" },
            new TestEvent { Data = "Event 2" }
        };

        var handler = new Mock<IEventHandler<TestEvent>>();
        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IEnumerable<IEventHandler<TestEvent>>)))
            .Returns(new[] { handler.Object });

        // Act
        await _eventBus.PublishAsync(events);

        // Assert
        handler.Verify(x => x.HandleAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    private class TestEvent : Event
    {
        public string Data { get; set; } = string.Empty;
    }
} 