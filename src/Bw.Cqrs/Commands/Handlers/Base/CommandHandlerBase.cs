using Bw.Cqrs.Command.Contract;
using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Common.Results;
using Microsoft.Extensions.Logging;

namespace Bw.Cqrs.Commands.Handlers.Base;

/// <summary>
/// Base class for command handlers with specific result type
/// </summary>
/// <typeparam name="TCommand">Type of command to handle</typeparam>
/// <typeparam name="TResult">Type of result</typeparam>
public abstract class CommandHandlerBase<TCommand, TResult> : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand
    where TResult : IResult
{
    /// <summary>
    /// Logger instance
    /// </summary>
    protected readonly ILogger<CommandHandlerBase<TCommand, TResult>> Logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandHandlerBase{TCommand, TResult}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    protected CommandHandlerBase(ILogger<CommandHandlerBase<TCommand, TResult>> logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the specified command asynchronously.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation (optional).</param>
    /// <returns>A task representing the asynchronous operation, containing the result of the command handling.</returns>
    public virtual async Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug(
                "[{CommandType}] Starting command execution",
                typeof(TCommand).Name);

            var result = await HandleCommandAsync(command, cancellationToken);

            Logger.LogDebug(
                "[{CommandType}] Command execution completed successfully",
                typeof(TCommand).Name);

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "[{CommandType}] Error executing command",
                typeof(TCommand).Name);
            throw;
        }
    }

    /// <summary>
    /// Handles the command execution
    /// </summary>
    /// <param name="command">Command to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of command execution</returns>
    protected abstract Task<TResult> HandleCommandAsync(TCommand command, CancellationToken cancellationToken);
}

/// <summary>
/// Base class for command handlers without specific result type
/// </summary>
/// <typeparam name="TCommand">Type of command to handle</typeparam>
public abstract class CommandHandlerBase<TCommand> : CommandHandlerBase<TCommand, IResult>, ICommandHandler<TCommand>
    where TCommand : ICommand
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CommandHandlerBase{TCommand}"/> class.
    /// </summary>
    /// <param name="logger"></param>
    protected CommandHandlerBase(ILogger<CommandHandlerBase<TCommand>> logger)
        : base(logger)
    {
    }
}