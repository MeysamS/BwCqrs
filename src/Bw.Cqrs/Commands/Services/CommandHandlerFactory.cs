using Bw.Cqrs.Command.Contract;
using Bw.Cqrs.Common.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Bw.Cqrs.Commands.Services;

/// <summary>
/// Default implementation of ICommandHandlerFactory
/// </summary>
public class CommandHandlerFactory : ICommandHandlerFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommandHandlerFactory> _logger;

    /// <summary>
    /// Initializes a new instance of the CommandHandlerFactory
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    /// <param name="logger">The logger instance</param>
    public CommandHandlerFactory(
        IServiceProvider serviceProvider,
        ILogger<CommandHandlerFactory> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public ICommandHandler<TCommand> Create<TCommand>() where TCommand : ICommand
    {
        try
        {
            _logger.LogDebug(
                "Creating command handler for command type {CommandType}",
                typeof(TCommand).Name);

            var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
            
            _logger.LogDebug(
                "Successfully created handler of type {HandlerType} for command {CommandType}",
                handler.GetType().Name,
                typeof(TCommand).Name);

            return handler;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error creating command handler for command type {CommandType}",
                typeof(TCommand).Name);
            throw;
        }
    }

    /// <inheritdoc />
    public ICommandHandler<TCommand, TResult> Create<TCommand, TResult>()
        where TCommand : ICommand
        where TResult : IResult
    {
        try
        {
            _logger.LogDebug(
                "Creating command handler for command type {CommandType} with result type {ResultType}",
                typeof(TCommand).Name,
                typeof(TResult).Name);

            var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResult>>();
            
            _logger.LogDebug(
                "Successfully created handler of type {HandlerType} for command {CommandType} with result {ResultType}",
                handler.GetType().Name,
                typeof(TCommand).Name,
                typeof(TResult).Name);

            return handler;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error creating command handler for command type {CommandType} with result type {ResultType}",
                typeof(TCommand).Name,
                typeof(TResult).Name);
            throw;
        }
    }
}