using Bw.Cqrs.InternalCommands.Postgres.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace Bw.Cqrs.Commands.Postgres.Tests.Integration;

public abstract class PostgresIntegrationTestBase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    protected readonly CommandDbContext DbContext;

    protected PostgresIntegrationTestBase()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithDatabase("cqrs_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

        var options = new DbContextOptionsBuilder<CommandDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        DbContext = new CommandDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await DbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
        await _container.DisposeAsync();
    }
} 