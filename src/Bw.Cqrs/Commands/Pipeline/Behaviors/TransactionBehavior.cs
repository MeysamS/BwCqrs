using System.Data;
using System.Data.Common;
using System.Transactions;
using Bw.Cqrs.Command.Contract;
using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Common.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bw.Cqrs.Commands.Pipeline.Behaviors;

/// <summary>
/// Pipeline behavior that wraps command handling in a transaction to ensure atomicity
/// </summary>
/// <typeparam name="TCommand">Type of the command</typeparam>
/// <typeparam name="TResult">Type of the result</typeparam>
public class TransactionBehavior<TCommand, TResult> : ICommandPipelineBehavior<TCommand, TResult>
    where TCommand : ICommand
    where TResult : IResult
{
    private readonly ILogger<TransactionBehavior<TCommand, TResult>> _logger;
    private readonly IDbTransaction? _currentTransaction;
    private readonly Configuration.TransactionOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionBehavior{TCommand, TResult}"/> class.
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="options">The transaction options</param>
    /// <param name="currentTransaction">Optional existing transaction to use (if null, a new transaction will be created)</param>
    public TransactionBehavior(
        ILogger<TransactionBehavior<TCommand, TResult>> logger,
        IOptions<Configuration.TransactionOptions> options,
        IDbTransaction? currentTransaction = null)
    {
        _logger = logger;
        _options = options.Value;
        _currentTransaction = currentTransaction;
    }

    /// <inheritdoc />
    public async Task<TResult> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken,
        CommandHandlerDelegate<TResult> next)
    {
        // If a transaction is already provided, use it without creating a new one
        if (_currentTransaction != null)
        {
            _logger.LogDebug(
                "Using existing database transaction for command {CommandType}",
                typeof(TCommand).Name);

            return await next();
        }

        // Create a new transaction scope
        _logger.LogDebug(
            "Creating transaction scope with isolation level {IsolationLevel} for command {CommandType}",
            _options.IsolationLevel,
            typeof(TCommand).Name);

        // Configure transaction options
        var transactionOptions = new System.Transactions.TransactionOptions
        {
            IsolationLevel = (System.Transactions.IsolationLevel)(int)_options.IsolationLevel,
            Timeout = _options.TimeoutSeconds > 0
                ? TimeSpan.FromSeconds(_options.TimeoutSeconds)
                : TimeSpan.MaxValue
        };

        using var transactionScope = new TransactionScope(
            TransactionScopeOption.Required,
            transactionOptions,
            TransactionScopeAsyncFlowOption.Enabled);

        try
        {
            // Execute the next behavior in the pipeline
            var result = await ExecuteWithPossibleRetryAsync(next, cancellationToken);

            // If the command was successful, complete the transaction
            if (result.IsSuccess)
            {
                _logger.LogDebug(
                    "Command {CommandType} executed successfully, completing transaction",
                    typeof(TCommand).Name);

                transactionScope.Complete();
            }
            else
            {
                _logger.LogWarning(
                    "Command {CommandType} failed with error: {ErrorMessage}. Transaction will be rolled back.",
                    typeof(TCommand).Name,
                    result.ErrorMessage);
                // No need to explicitly roll back - it happens automatically when scope is disposed without Complete()
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error executing command {CommandType}. Transaction will be rolled back.",
                typeof(TCommand).Name);

            // Rethrow the exception to be handled by error handling behavior
            throw;
        }
    }

    private async Task<TResult> ExecuteWithPossibleRetryAsync(
        CommandHandlerDelegate<TResult> next,
        CancellationToken cancellationToken)
    {
        int retryCount = 0;

        while (true)
        {
            try
            {
                return await next();
            }
            catch (DbException ex) when (IsDeadlockException(ex) &&
                                        _options.RetryOnDeadlock &&
                                        retryCount < _options.MaxDeadlockRetries)
            {
                retryCount++;

                _logger.LogWarning(
                    ex,
                    "Deadlock detected when executing command {CommandType}. Retry attempt {RetryCount} of {MaxRetries}",
                    typeof(TCommand).Name,
                    retryCount,
                    _options.MaxDeadlockRetries);

                await Task.Delay(_options.DeadlockRetryDelayMs, cancellationToken);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    private bool IsDeadlockException(DbException exception)
    {
        // SQL Server deadlock victim error code
        const int sqlServerDeadlockErrorNumber = 1205;

        // PostgreSQL deadlock error code
        const string postgresDeadlockCode = "40P01";

        // Try to detect deadlock based on provider-specific error codes
        // This can be extended to support other database providers
        return exception.Message.Contains("deadlock", StringComparison.OrdinalIgnoreCase) ||
               (exception.GetType().Name.Contains("SqlException") &&
                (exception.GetType().GetProperty("Number")?.GetValue(exception)?.ToString() == sqlServerDeadlockErrorNumber.ToString())) ||
               (exception.GetType().Name.Contains("NpgsqlException") &&
                (exception.GetType().GetProperty("SqlState")?.GetValue(exception)?.ToString() == postgresDeadlockCode));
    }
}

/// <summary>
/// Specialized transaction behavior for commands that return IResult type
/// </summary>
/// <typeparam name="TCommand">Type of the command</typeparam>
public class TransactionBehavior<TCommand> : TransactionBehavior<TCommand, IResult>, ICommandPipelineBehavior<TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionBehavior{TCommand}"/> class.
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="options">The transaction options</param>
    /// <param name="currentTransaction">Optional existing transaction to use (if null, a new transaction will be created)</param>
    public TransactionBehavior(
        ILogger<TransactionBehavior<TCommand, IResult>> logger,
        IOptions<Configuration.TransactionOptions> options,
        IDbTransaction? currentTransaction = null)
        : base(logger, options, currentTransaction)
    {
    }
}
