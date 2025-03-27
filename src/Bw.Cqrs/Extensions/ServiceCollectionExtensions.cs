﻿using System.Reflection;
using Bw.Cqrs.Command.Contract;
using Bw.Cqrs.Commands.Pipeline.Behaviors;
using Bw.Cqrs.Commands.Services;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using Bw.Cqrs.Configuration;
using Bw.Cqrs.Events.Contracts;
using Bw.Cqrs.Events.Services;

namespace Bw.Cqrs.Extensions;

public static class ServiceCollectionExtensions
{
    public static ICqrsBuilder AddBwCqrs(this IServiceCollection services, Action<ICqrsBuilder> configure, params Assembly[] assemblies)
    {
        // Register core services
        services.AddScoped<ICommandBus, DefaultCommandBus>();
        services.AddScoped<ICommandHandlerFactory, CommandHandlerFactory>();
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

    public static ICqrsBuilder AddErrorHandling(this ICqrsBuilder builder)
    {
        builder.Services.AddScoped(typeof(ErrorHandlingBehavior<,>));
        return builder;
    }

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

    public static ICqrsBuilder AddValidation(this ICqrsBuilder builder)
    {
        builder.Services.AddScoped(typeof(ValidationBehavior<,>));
        return builder;
    }

    public static ICqrsBuilder AddLogging(this ICqrsBuilder builder)
    {
        builder.Services.AddScoped(typeof(LoggingBehavior<,>));
        return builder;
    }

    public static ICqrsBuilder AddEventHandling(this ICqrsBuilder builder)
    {
        builder.Services.AddScoped<IEventBus, InMemoryEventBus>();
        
        // Register event handlers
        builder.Services.Scan(scan => scan
            .FromAssemblies(builder.Assemblies)
            .AddClasses(classes => classes
                .AssignableTo(typeof(IEventHandler<>)))
            .AsClosedTypeOf(typeof(IEventHandler<>))
            .WithScopedLifetime());

        return builder;
    }
}

public static class ServiceTypeSelectorExtensions
{
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