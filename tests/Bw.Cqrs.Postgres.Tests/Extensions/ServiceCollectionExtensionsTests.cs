using Bw.Cqrs.Commands.Services;
using Bw.Cqrs.Postgres.Data;
using Bw.Cqrs.Postgres.Extensions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Bw.Cqrs.Postgres.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddPostgresCqrs_ShouldRegisterRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test;Username=test;Password=test";

        // Act
        services.AddPostgresCqrs(connectionString);

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        serviceProvider.GetService<CqrsDbContext>().Should().NotBeNull();
        serviceProvider.GetService<IInternalCommandStore>().Should().NotBeNull();
    }
} 