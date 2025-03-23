using Bw.Cqrs.Commands.Services;
using Bw.Cqrs.Postgres.Data;
using Bw.Cqrs.Postgres.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bw.Cqrs.Postgres.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostgresCqrs(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<CqrsDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IInternalCommandStore, PostgresInternalCommandStore>();

        return services;
    }
} 