using System.Reflection;
using Bw.Cqrs.Command.Contract;
using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Commands.Pipeline.Behaviors;
using Bw.Cqrs.Common.Results;
using Microsoft.Extensions.DependencyInjection;

namespace Bw.Cqrs.Configuration;

/// <summary>
/// Represents a builder for the CQRS framework
/// </summary>
public class CqrsBuilder : ICqrsBuilder
{
    /// <summary>
    /// Gets the service collection
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Gets the assemblies
    /// </summary>
    public IEnumerable<Assembly> Assemblies { get; }

    /// <summary>
    /// Initializes a new instance of the CqrsBuilder class
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assemblies">The assemblies</param>
    public CqrsBuilder(IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
        Assemblies = assemblies ?? throw new ArgumentNullException(nameof(assemblies));
    }

    /// <summary>
    /// Adds validation support to the CQRS framework
    /// </summary>
    /// <returns>The CqrsBuilder instance</returns>
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

    /// <summary>
    /// Adds logging support to the CQRS framework
    /// </summary>
    /// <returns>The CqrsBuilder instance</returns>
    public CqrsBuilder AddLogging()
    {
        Services.AddScoped(typeof(ICommandPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        return this;
    }

    /// <summary>
    /// Adds a custom behavior to the CQRS framework
    /// </summary>
    /// <typeparam name="TBehavior">The type of behavior</typeparam>
    /// <typeparam name="TCommand">The type of command</typeparam>
    /// <typeparam name="TResult">The type of result</typeparam>
    /// <returns>The CqrsBuilder instance</returns>
    public CqrsBuilder AddCustomBehavior<TBehavior, TCommand, TResult>()
        where TBehavior : class, ICommandPipelineBehavior<TCommand, TResult>
        where TCommand : ICommand
        where TResult : IResult
    {
        Services.AddScoped(typeof(ICommandPipelineBehavior<TCommand, TResult>), typeof(TBehavior));
        return this;
    }
}
