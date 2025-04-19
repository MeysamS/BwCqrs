using System.Reflection;
using Bw.Cqrs.Command.Contract;
using Bw.Cqrs.Commands.Pipeline.Behaviors;
using Bw.Cqrs.Commands.Services;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using Bw.Cqrs.Configuration;
using Bw.Cqrs.Events.Contracts;
using Bw.Cqrs.Events.Services;
using Bw.Cqrs.Commands.Configuration;
using Bw.Cqrs.Commands.Contracts;
using Microsoft.Extensions.Options;
using System.Data;
using Bw.Cqrs.Queries.Contracts;
using Bw.Cqrs.Queries.Services;

namespace Bw.Cqrs.Extensions;

/// <summary>
/// Extension methods for the IServiceCollection interface
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds event handling support to the CQRS builder
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">The configuration action</param>
    /// <param name="assemblies">The assemblies to scan</param>
    /// <returns>The CQRS builder for method chaining</returns>
    public static ICqrsBuilder AddBwCqrs(this IServiceCollection services, Action<ICqrsBuilder> configure, params Assembly[] assemblies)
    {
        // Register core services
        services.AddScoped<ICommandProcessor, CommandProccesor>();
        services.AddScoped<IQueryProcessor, QueryProcessor>();
        services.AddScoped<ICommandHandlerFactory, CommandHandlerFactory>();
        services.AddScoped<IQueryHandlerFactory, QueryHandlerFactory>();
        services.AddScoped<IInternalCommandStore, InMemoryInternalCommandStore>();

        // Register command handlers
        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes
                .AssignableTo(typeof(ICommandHandler<>)))
            .AsClosedTypeOf(typeof(ICommandHandler<>))
            .WithScopedLifetime());

        var builder = new CqrsBuilder(services, assemblies);
        configure?.Invoke(builder);

        return builder;
    }

    /// <summary>
    /// Adds error handling support to the CQRS builder
    /// </summary>
    /// <param name="builder">The CQRS builder instance</param>
    /// <returns>The CQRS builder for method chaining</returns>
    public static ICqrsBuilder AddErrorHandling(this ICqrsBuilder builder)
    {
        builder.Services.AddScoped(typeof(ErrorHandlingBehavior<,>));
        return builder;
    }

    /// <summary>
    /// Adds retry support to the CQRS builder
    /// </summary>
    /// <param name="builder">The CQRS builder instance</param>
    /// <param name="maxRetries">The maximum number of retries</param>
    /// <param name="delayMilliseconds">The delay in milliseconds between retries</param>
    public static ICqrsBuilder AddRetry(this ICqrsBuilder builder, int maxRetries = 3, int delayMilliseconds = 1000)
    {
        builder.Services.AddScoped(typeof(RetryBehavior<,>));
        builder.Services.Configure<RetryOptions>(options =>
        {
            options.MaxRetries = maxRetries;
            options.DelayMilliseconds = delayMilliseconds;
        });
        return builder;
    }

    /// <summary>
    /// Adds validation support to the CQRS builder
    /// </summary>
    /// <param name="builder">The CQRS builder instance</param>
    /// <returns>The CQRS builder for method chaining</returns>
    public static ICqrsBuilder AddValidation(this ICqrsBuilder builder)
    {
        builder.Services.AddScoped(typeof(ValidationBehavior<,>));
        return builder;
    }

    /// <summary>
    /// Adds logging support to the CQRS builder
    /// </summary>
    /// <param name="builder">The CQRS builder instance</param>
    /// <returns>The CQRS builder for method chaining</returns> 
    public static ICqrsBuilder AddLogging(this ICqrsBuilder builder)
    {
        builder.Services.AddScoped(typeof(LoggingBehavior<,>));
        return builder;
    }

    /// <summary>
    /// Adds transaction support to the CQRS builder
    /// </summary>
    /// <param name="builder">The CQRS builder instance</param>
    /// <param name="configureOptions">Optional action to configure transaction options</param>
    /// <returns>The CQRS builder for method chaining</returns>
    public static ICqrsBuilder AddTransactionSupport(
        this ICqrsBuilder builder,
        Action<TransactionOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        // Configure transaction options
        if (configureOptions != null)
        {
            builder.Services.Configure(configureOptions);
        }
        else
        {
            builder.Services.Configure<TransactionOptions>(options => { });
        }

        // Register both versions of the transaction behavior
        builder.Services.AddScoped(typeof(TransactionBehavior<,>));
        builder.Services.AddScoped(typeof(TransactionBehavior<>));

        return builder;
    }

    /// <summary>
    /// Adds event handling support to the CQRS builder
    /// </summary>
    /// <param name="builder">The CQRS builder instance</param>
    /// <returns>The CQRS builder for method chaining</returns>
    public static ICqrsBuilder AddEventHandling(this ICqrsBuilder builder)
    {
        builder.Services.AddScoped<IEventProcessor, InMemoryEventBus>();

        // Register event handlers
        builder.Services.Scan(scan => scan
            .FromAssemblies(builder.Assemblies)
            .AddClasses(classes => classes
                .AssignableTo(typeof(IEventHandler<>)))
            .AsClosedTypeOf(typeof(IEventHandler<>))
            .WithScopedLifetime());

        return builder;
    }

    /// <summary>
    /// Adds internal command processing support
    /// </summary>
    public static InternalCommandStorageBuilder AddInternalCommands(
        this ICqrsBuilder builder,
        Action<InternalCommandOptions>? configureOptions = null)
    {
        // Configure options
        if (configureOptions != null)
        {
            builder.Services.Configure(configureOptions);
        }
        else
        {
            builder.Services.Configure<InternalCommandOptions>(options => { });
        }

        // Add the processor service
        builder.Services.AddHostedService<InternalCommandProcessor>();

        return new InternalCommandStorageBuilder(builder.Services, builder);
    }
}

/// <summary>
/// Extension methods for the IServiceTypeSelector interface
/// </summary>
public static class ServiceTypeSelectorExtensions
{
    /// <summary>
    /// Adds a service type selector that matches closed generic types
    /// </summary>
    /// <param name="selector">The service type selector</param>
    /// <param name="closeType">The closed type to match</param>
    /// <returns>The service type selector for method chaining</returns>
    public static ILifetimeSelector AsClosedTypeOf(this IServiceTypeSelector selector, Type closeType)
    {
        return _ = selector.As(t =>
        {
            var types = t.GetInterfaces()
                .Where(p => p.IsGenericType && p.GetGenericTypeDefinition() == closeType)
                .Select(
                    implementedInterface =>
                        implementedInterface.GenericTypeArguments.Any(x => x.IsTypeDefinition)
                            ? implementedInterface
                            : implementedInterface.GetGenericTypeDefinition()
                )
                .Distinct();
            var result = types.ToList();
            return result;
        });
    }
}