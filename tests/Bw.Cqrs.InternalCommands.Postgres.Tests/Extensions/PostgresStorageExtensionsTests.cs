using Bw.Cqrs.Commands.Configuration;
using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.InternalCommands.Postgres.Extensions;
using Bw.Cqrs.InternalCommands.Postgres.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Bw.Cqrs.Configuration;
using System.Reflection;

namespace Bw.Cqrs.Commands.Postgres.Tests.Extensions;

public class PostgresStorageExtensionsTests
{
    [Fact]
    public void UsePostgres_ShouldRegisterRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var cqrsBuilder = new CqrsBuilder(services, new[] { Assembly.GetExecutingAssembly() });
        var builder = new InternalCommandStorageBuilder(services, cqrsBuilder);

        // Act
        builder.UsePostgres(options =>
        {
            options.ConnectionString = "Host=localhost;Database=test;Username=test;Password=test";
            options.CommandTimeout = TimeSpan.FromSeconds(30);
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var store = serviceProvider.GetService<IInternalCommandStore>();
        Assert.NotNull(store);
        Assert.IsType<PostgresInternalCommandStore>(store);
    }

    [Fact]
    public void UsePostgres_WithInvalidOptions_ShouldThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var cqrsBuilder = new CqrsBuilder(services, new[] { Assembly.GetExecutingAssembly() });
        var builder = new InternalCommandStorageBuilder(services, cqrsBuilder);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            builder.UsePostgres(options =>
            {
                options.ConnectionString = string.Empty;
                options.CommandTimeout = TimeSpan.FromSeconds(0);
            }));
    }
} 