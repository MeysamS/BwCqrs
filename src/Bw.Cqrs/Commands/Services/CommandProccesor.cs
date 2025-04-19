using System.Reflection;
using Bw.Cqrs.Command.Contract;
using Bw.Cqrs.Commands.Base;
using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Common.Results;
using Microsoft.Extensions.DependencyInjection;

namespace Bw.Cqrs.Commands.Services;

/// <summary>
/// Represents the default command bus
/// </summary>
public class CommandProccesor : ICommandProcessor
{
    private readonly ICommandHandlerFactory _commandHandlerFactory;
    private readonly IInternalCommandStore _internalCommandStore;

    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the DefaultCommandBus class
    /// </summary>
    /// <param name="commandHandlerFactory">The command handler factory</param>
    /// <param name="internalCommandStore">The internal command store</param>
    /// <param name="serviceProvider">The service provider</param>
    public CommandProccesor(
        ICommandHandlerFactory commandHandlerFactory,
        IInternalCommandStore internalCommandStore,
        IServiceProvider serviceProvider)
    {
        _commandHandlerFactory = commandHandlerFactory ?? throw new ArgumentNullException(nameof(commandHandlerFactory));
        _internalCommandStore = internalCommandStore ?? throw new ArgumentNullException(nameof(internalCommandStore));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Dispatches a command
    /// </summary>
    /// <typeparam name="TCommand">The type of command</typeparam>
    /// <param name="command">The command</param>
    /// <returns>A task</returns>
    public async Task DispatchAsync<TCommand>(TCommand command)
        where TCommand : ICommand
    {
        if (typeof(TCommand).IsInterface || typeof(TCommand).IsAbstract)
        {
            var commandType = command.GetType();
            var method = typeof(CommandProccesor)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m =>
                    m.Name == nameof(DispatchAsync) &&
                    m.IsGenericMethodDefinition &&
                    m.GetGenericArguments().Length == 1 &&
                    m.GetParameters().Length == 1)
                            ?.MakeGenericMethod(commandType);
            if (method == null)
            {
                throw new InvalidOperationException($"Cannot find generic DispatchAsync method for type {commandType.Name}");
            }
            var task = (Task)method.Invoke(this, new object[] { command })!;
            await task;
            return;
        }
        var handler = _commandHandlerFactory.Create<TCommand>();
        var behaviors = _serviceProvider.GetServices<ICommandPipelineBehavior<TCommand, IResult>>();

        // Build the pipeline
        CommandHandlerDelegate<IResult> pipeline = () => handler.HandleAsync(command);

        foreach (var behavior in behaviors.Reverse())
        {
            var currentPipeline = pipeline;
            pipeline = () => behavior.HandleAsync(command, default, currentPipeline);
        }

        await pipeline();
    }

    /// <summary>
    /// Dispatches a command and returns a result
    /// </summary>
    /// <typeparam name="TCommand">The type of command</typeparam>
    /// <typeparam name="TResult">The type of result</typeparam>
    /// <param name="command">The command</param>
    /// <returns>The result</returns>
    public async Task<TResult?> DispatchAsync<TCommand, TResult>(TCommand command)
        where TCommand : ICommand
        where TResult : class, IResult
    {

        if (typeof(TCommand).IsInterface || typeof(TCommand).IsAbstract ||
            typeof(TResult).IsInterface || typeof(TResult).IsAbstract)
        {
            var commandType = command.GetType();

            var method = typeof(CommandProccesor)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m =>
                    m.Name == nameof(DispatchAsync) &&
                    m.IsGenericMethodDefinition &&
                    m.GetGenericArguments().Length == 2 &&
                    m.GetParameters().Length == 2)
                            ?.MakeGenericMethod(commandType, typeof(TResult));

            if (method == null)
            {
                throw new InvalidOperationException($"Cannot find generic DispatchAsync method for type {commandType.Name} with result {typeof(TResult).Name}");
            }

            var task = (Task<TResult?>)method.Invoke(this, new object[] { command })!;
            return await task;
        }
        var handler = _commandHandlerFactory.Create<TCommand, TResult>();
        var behaviors = _serviceProvider.GetServices<ICommandPipelineBehavior<TCommand, TResult>>();

        // Build the pipeline
        CommandHandlerDelegate<TResult> pipeline = () => handler.HandleAsync(command);

        foreach (var behavior in behaviors.Reverse())
        {
            var currentPipeline = pipeline;
            pipeline = () => behavior.HandleAsync(command, default, currentPipeline);
        }

        return await pipeline();
    }

    /// <summary>
    /// Dispatches a command
    /// </summary>
    /// <typeparam name="TCommand">The type of command</typeparam>
    /// <param name="command">The command</param>   
    public void Dispatch<TCommand>(TCommand command)
        where TCommand : ICommand
    {
        DispatchAsync(command).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Schedules a command to be processed asynchronously.
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    /// <param name="command"></param>
    /// <returns></returns>
    public async Task ScheduleAsync<TCommand>(TCommand command) where TCommand : InternalCommand
    {
       await _internalCommandStore.SaveAsync(command);
    }


}