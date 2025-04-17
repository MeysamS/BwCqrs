using Bw.Cqrs.Command;
using Bw.Cqrs.Command.Contract;
using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Common.Results;
using Microsoft.Extensions.Logging;

namespace Bw.Cqrs.Commands.Pipeline.Behaviors;

/// <summary>
/// Provides error handling behavior for command execution pipeline
/// </summary>
/// <typeparam name="TCommand">Type of command</typeparam>
/// <typeparam name="TResult">Type of result</typeparam>
public class ErrorHandlingBehavior<TCommand, TResult> : ICommandPipelineBehavior<TCommand, TResult>
    where TCommand : ICommand
    where TResult : IResult
{
    private readonly ILogger<ErrorHandlingBehavior<TCommand, TResult>> _logger;

    /// <summary>
    /// Initializes a new instance of the error handling behavior
    /// </summary>
    /// <param name="logger">Logger instance</param>
    public ErrorHandlingBehavior(ILogger<ErrorHandlingBehavior<TCommand, TResult>> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles the command execution with error handling
    /// </summary>
    /// <param name="command">Command to handle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="next">Next handler in the pipeline</param>
    /// <returns>Result of the command execution</returns>
    public async Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken, CommandHandlerDelegate<TResult> next)
    {
        try
        {
            return await next();
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed for command {CommandType}", typeof(TCommand).Name);
            return CreateErrorResult<TResult>(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing command {CommandType}", typeof(TCommand).Name);
            return CreateErrorResult<TResult>("An unexpected error occurred while processing the command");
        }
    }

    /// <summary>
    /// Creates an error result of the specified type
    /// </summary>
    private static TResult CreateErrorResult<T>(string message) where T : IResult
    {
        if (typeof(T) == typeof(IResult))
        {
            return (TResult)(IResult)CommandResult.Failure(message);
        }

        var resultType = typeof(T);
        var constructor = resultType.GetConstructor(new[] { typeof(bool), typeof(string) });
        
        if (constructor != null)
        {
            return (TResult)constructor.Invoke(new object[] { false, message });
        }

        throw new InvalidOperationException($"Cannot create error result of type {typeof(T).Name}");
    }
} 