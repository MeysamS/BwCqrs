using Bw.Cqrs.Commands.Configuration;
using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Configuration;
using Bw.Cqrs.InternalCommands.Postgres.Models;
using Bw.Cqrs.InternalCommands.Postgres.Services;
using Bw.Cqrs.InternalCommands.Postgres.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bw.Cqrs.InternalCommands.Postgres.Extensions;

public static class PostgresStorageExtensions
{
    public static ICqrsBuilder UsePostgres(
        this InternalCommandStorageBuilder builder,
        Action<PostgresOptions> configure)
    {
        builder.Services.Configure(configure);

        builder.Services.AddSingleton(sp =>
        {
            var options = new PostgresOptions();
            configure(options);
            return options;
        });

        builder.Services.AddDbContext<CommandDbContext>((sp, options) =>
        {
            var postgresOptions = sp.GetRequiredService<PostgresOptions>();
            options.UsePostgres(postgresOptions);
        });

        builder.Services.AddScoped<IInternalCommandStore, PostgresInternalCommandStore>();

        return builder.Builder;
    }

    public static DbContextOptionsBuilder UsePostgres(
        this DbContextOptionsBuilder builder,
        PostgresOptions options)
    {
        return builder.UseNpgsql(
            options.ConnectionString,
            npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure();
                npgsqlOptions.CommandTimeout((int)options.CommandTimeout.TotalSeconds);
            });
    }
} 