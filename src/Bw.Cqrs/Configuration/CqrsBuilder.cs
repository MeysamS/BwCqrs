using System.Reflection;
using Bw.Cqrs.Command.Contract;
using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Commands.Pipeline.Behaviors;
using Bw.Cqrs.Common.Results;
using Microsoft.Extensions.DependencyInjection;

namespace Bw.Cqrs.Configuration;

public class CqrsBuilder : ICqrsBuilder
{
    public IServiceCollection Services { get; }
    public IEnumerable<Assembly> Assemblies { get; }

    public CqrsBuilder(IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
        Assemblies = assemblies ?? throw new ArgumentNullException(nameof(assemblies));
    }

    public CqrsBuilder AddValidation()
    {
        Services.Scan(scan =>
            scan.FromAssemblies(Assemblies)
                .AddClasses(classes => classes.AssignableTo(typeof(IValidationHandler<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());
        
        Services.AddScoped(typeof(ICommandPipelineBehavior<>), typeof(ValidationBehavior<>));
        return this;
    }

    public CqrsBuilder AddLogging()
    {
        Services.AddScoped(typeof(ICommandPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        return this;
    }

    public CqrsBuilder AddCustomBehavior<TBehavior, TCommand, TResult>()
        where TBehavior : class, ICommandPipelineBehavior<TCommand, TResult>
        where TCommand : ICommand
        where TResult : IResult
    {
        Services.AddScoped(typeof(ICommandPipelineBehavior<TCommand, TResult>), typeof(TBehavior));
        return this;
    }
}
