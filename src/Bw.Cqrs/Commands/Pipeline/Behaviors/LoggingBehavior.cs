using System.Diagnostics;
using Bw.Cqrs.Commands.Base;
using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Common.Results;
using Microsoft.Extensions.Logging;

namespace Bw.Cqrs.Commands.Pipeline.Behaviors;

/// <summary>
/// Provides logging behavior for command execution pipeline
/// </summary>
/// <typeparam name="TCommand">Type of command</typeparam>
/// <typeparam name="TResult">Type of result</typeparam>
public class LoggingBehavior<TCommand, TResult> : ICommandPipelineBehavior<TCommand, TResult>
    where TCommand : CommandBase
    where TResult : IResult
{
    private readonly ILogger<LoggingBehavior<TCommand, TResult>> _logger;

    /// <summary>
    /// Initializes a new instance of the logging behavior
    /// </summary>
    /// <param name="logger">Logger instance</param>
    public LoggingBehavior(ILogger<LoggingBehavior<TCommand, TResult>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the command execution with logging
    /// </summary>
    public async Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken, CommandHandlerDelegate<TResult> next)
    {
        var commandType = command.GetType().Name;
        var commandId = command.Id;

        try
        {
            _logger.LogInformation(
                "[{CommandType}] ({CommandId}) Processing command started",
                commandType,
                commandId);

            var sw = Stopwatch.StartNew();
            var result = await next();
            sw.Stop();

            _logger.LogInformation(
                "[{CommandType}] ({CommandId}) Processing command completed in {ElapsedMilliseconds}ms",
                commandType,
                commandId,
                sw.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[{CommandType}] ({CommandId}) Processing command failed",
                commandType,
                commandId);
            throw;
        }
    }
}

/// <summary>
/// Provides logging behavior for command execution pipeline without specific result type
/// </summary>
/// <typeparam name="TCommand">Type of command</typeparam>
public class LoggingBehavior<TCommand> : LoggingBehavior<TCommand, IResult>, ICommandPipelineBehavior<TCommand>
    where TCommand : CommandBase
{
    /// <summary>
    /// Initializes a new instance of the logging behavior
    /// </summary>
    public LoggingBehavior(ILogger<LoggingBehavior<TCommand, IResult>> logger) 
        : base(logger)
    {
    }
} 