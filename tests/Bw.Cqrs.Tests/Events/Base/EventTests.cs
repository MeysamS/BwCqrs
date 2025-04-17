using Bw.Cqrs.Events.Base;
using FluentAssertions;

namespace Bw.Cqrs.Tests.Events.Base;

public class EventTests
{
    [Fact]
    public void Constructor_ShouldInitialize_IdAndOccurredOn()
    {
        // Arrange & Act
        var @event = new TestEvent();

        // Assert
        @event.Id.Should().NotBe(Guid.Empty);
        @event.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    private class TestEvent : Event
    {
        public string Data { get; set; } = string.Empty;
    }
} 