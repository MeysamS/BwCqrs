using Bw.Cqrs.Events.Base;
using Bw.Cqrs.Events.Contracts;
using Bw.Cqrs.Events.Services;
using Bw.Cqrs.Extensions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Bw.Cqrs.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddEventHandling_ShouldRegisterRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBwCqrs(builder =>
        {
            builder.AddEventHandling();
        }, typeof(ServiceCollectionExtensionsTests).Assembly);

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<IEventProcessor>().Should().NotBeNull();
        serviceProvider.GetService<IEventProcessor>().Should().BeOfType<InMemoryEventBus>();
    }

    [Fact]
    public void AddEventHandling_ShouldRegisterEventHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBwCqrs(builder =>
        {
            builder.AddEventHandling();
        }, typeof(TestEventHandler).Assembly);

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var handlers = serviceProvider.GetServices<IEventHandler<TestEvent>>();
        handlers.Should().NotBeEmpty();
        handlers.Should().Contain(x => x.GetType() == typeof(TestEventHandler));
    }

    private class TestEvent : Event
    {
    }

    private class TestEventHandler : IEventHandler<TestEvent>
    {
        public Task HandleAsync(TestEvent @event, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
} 