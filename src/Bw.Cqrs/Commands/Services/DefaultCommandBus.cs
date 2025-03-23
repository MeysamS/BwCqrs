using Bw.Cqrs.Command.Contract;
using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Common.Results;
using Microsoft.Extensions.DependencyInjection;

namespace Bw.Cqrs.Commands.Services;

public class DefaultCommandBus : ICommandBus
{
    private readonly ICommandHandlerFactory _commandHandlerFactory;
    private readonly IInternalCommandStore _internalCommandStore;

    private readonly IServiceProvider _serviceProvider;

    public DefaultCommandBus(
        ICommandHandlerFactory commandHandlerFactory,
        IInternalCommandStore internalCommandStore,
        IServiceProvider serviceProvider)
    {
        _commandHandlerFactory = commandHandlerFactory ?? throw new ArgumentNullException(nameof(commandHandlerFactory));
        _internalCommandStore = internalCommandStore ?? throw new ArgumentNullException(nameof(internalCommandStore));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task DispatchAsync<TCommand>(TCommand command)
        where TCommand : ICommand
    {
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

    public async Task<TResult?> DispatchAsync<TCommand, TResult>(TCommand command)
        where TCommand : ICommand
        where TResult : class, IResult
    {
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

    public void Dispatch<TCommand>(TCommand command)
        where TCommand : ICommand
    {
        DispatchAsync(command).GetAwaiter().GetResult();
    }



    public async Task ScheduleAsync<TCommand>(TCommand command)
    where TCommand : IInternalCommand
    {
        await _internalCommandStore.SaveAsync(command);
    }
}