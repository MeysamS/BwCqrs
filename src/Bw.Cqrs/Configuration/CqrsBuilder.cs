using System.Reflection;
using Bw.Cqrs.Command.Contract;
using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Commands.Pipeline.Behaviors;
using Bw.Cqrs.Common.Results;
using Microsoft.Extensions.DependencyInjection;

namespace Bw.Cqrs.Configuration;

public class CqrsBuilder : ICqrsBuilder
{
    private readonly IServiceCollection _services;
    private readonly Assembly[] _assemblies;

    public IServiceCollection Services => _services;

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
