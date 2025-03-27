using Bw.Cqrs.Command.Contract;
using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Common.Results;
using Microsoft.Extensions.Logging;

namespace Bw.Cqrs.Commands.Pipeline.Behaviors;

public class RetryBehavior<TCommand, TResult> : ICommandPipelineBehavior<TCommand, TResult>
    where TCommand : ICommand
    where TResult : IResult
{
    private readonly ILogger<RetryBehavior<TCommand, TResult>> _logger;
    private readonly int _maxRetries;
    private readonly TimeSpan _delay;

    public RetryBehavior(
        ILogger<RetryBehavior<TCommand, TResult>> logger,
        int maxRetries = 3,
        int delayMilliseconds = 1000)
    {
        _logger = logger;
        _maxRetries = maxRetries;
        _delay = TimeSpan.FromMilliseconds(delayMilliseconds);
    }

    public async Task<TResult> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken,
        CommandHandlerDelegate<TResult> next)
    {
        for (int i = 0; i <= _maxRetries; i++)
        {
            try
            {
                return await next();
            }
            catch (Exception ex) when (i < _maxRetries && IsRetryable(ex))
            {
                _logger.LogWarning(
                    ex,
                    "Retry attempt {RetryCount} of {MaxRetries} for command {CommandType}",
                    i + 1,
                    _maxRetries,
                    typeof(TCommand).Name);

                await Task.Delay(_delay, cancellationToken);
            }
        }

        throw new CommandRetryException($"Command {typeof(TCommand).Name} failed after {_maxRetries} retries");
    }

    private bool IsRetryable(Exception ex)
    {
        // Add logic to determine which exceptions should trigger a retry
        return ex is TimeoutException
            || ex is System.Net.Sockets.SocketException
            || ex is System.IO.IOException;
    }
}

public class CommandRetryException : Exception
{
    public CommandRetryException(string message) : base(message)
    {
    }
} 