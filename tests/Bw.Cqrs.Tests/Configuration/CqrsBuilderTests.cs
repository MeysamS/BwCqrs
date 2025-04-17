using Bw.Cqrs.Command.Base.Commands;
using Bw.Cqrs.Commands.Pipeline.Behaviors;
using Bw.Cqrs.Common.Results;
using Bw.Cqrs.Extensions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Bw.Cqrs.Tests.Configuration;

public class CqrsBuilderTests
{
    private readonly IServiceCollection _services;

    public CqrsBuilderTests()
    {
        _services = new ServiceCollection();
    }

    [Fact]
    public void AddBwCqrs_ShouldRegisterAllBehaviors_WhenAllFeaturesEnabled()
    {
        // Arrange & Act
        _services.AddBwCqrs(builder =>
        {
            builder
                .AddValidation()
                .AddLogging()
                .AddErrorHandling()
                .AddRetry();
        }, typeof(CqrsBuilderTests).Assembly);

        var provider = _services.BuildServiceProvider();

        // Assert
        provider.GetService<ValidationBehavior<TestCommand, IResult>>().Should().NotBeNull();
        provider.GetService<LoggingBehavior<TestCommand, IResult>>().Should().NotBeNull();
        provider.GetService<ErrorHandlingBehavior<TestCommand, IResult>>().Should().NotBeNull();
        provider.GetService<RetryBehavior<TestCommand, IResult>>().Should().NotBeNull();
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