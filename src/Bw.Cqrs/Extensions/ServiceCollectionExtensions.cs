using System.Reflection;
using Bw.Cqrs.Command.Contract;
using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Commands.Pipeline.Behaviors;
using Bw.Cqrs.Commands.Services;
using Bw.Cqrs.Common.Results;
using Bw.Cqrs.Queries.Contracts;
using Bw.Cqrs.Queries.Services;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace Bw.Cqrs.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCqrs(
        this IServiceCollection services,
        Action<CqrsBuilder> configure,
        params Assembly[] assemblies)
    {
        var builder = new CqrsBuilder(services, assemblies);
        configure?.Invoke(builder);

        // Register Command Handlers
        services.Scan(scan =>
            scan.FromAssemblies(assemblies)
                .AddClasses(classes => classes.Where(type =>
                    type.IsAssignableTo(typeof(ICommandHandler<,>)) || 
                    type.IsAssignableTo(typeof(ICommandHandler<>))))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        // Register Query Handlers
        services.Scan(scan =>
            scan.FromAssemblies(assemblies)
                .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        // Register Core Services
        services.AddScoped<ICommandHandlerFactory, CommandHandlerFactory>();
        services.AddScoped<ICommandBus, DefaultCommandBus>();
        services.AddScoped<IQueryHandlerFactory, QueryHandlerFactory>();
        services.AddScoped<IQueryBus, DefaultQueryBus>();

        return services;
    }
}

public class CqrsBuilder
{
    private readonly IServiceCollection _services;
    private readonly Assembly[] _assemblies;

    public CqrsBuilder(IServiceCollection services, Assembly[] assemblies)
    {
        _services = services;
        _assemblies = assemblies;
    }

    public CqrsBuilder AddValidation()
    {
        _services.Scan(scan =>
            scan.FromAssemblies(_assemblies)
                .AddClasses(classes => classes.AssignableTo(typeof(IValidationHandler<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());
        
        _services.AddScoped(typeof(ICommandPipelineBehavior<>), typeof(ValidationBehavior<>));
        return this;
    }

    public CqrsBuilder AddLogging()
    {
        _services.AddScoped(typeof(ICommandPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        return this;
    }

    public CqrsBuilder AddCustomBehavior<TBehavior, TCommand, TResult>()
        where TBehavior : class, ICommandPipelineBehavior<TCommand, TResult>
        where TCommand : ICommand
        where TResult : IResult
    {
        _services.AddScoped(typeof(ICommandPipelineBehavior<TCommand, TResult>), typeof(TBehavior));
        return this;
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