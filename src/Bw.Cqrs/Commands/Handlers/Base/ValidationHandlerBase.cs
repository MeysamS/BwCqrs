using Bw.Cqrs.Command;
using Bw.Cqrs.Command.Contract;
using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Common.Results;

namespace Bw.Cqrs.Commands.Base;

/// <summary>
/// Base class for validation handlers that implement IValidationHandler{TCommand}
/// </summary>
/// <typeparam name="TCommand">The type of command to validate</typeparam>
public abstract class ValidationHandlerBase<TCommand> : IValidationHandler<TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// Validates the command
    /// </summary>
    /// <param name="command">The command to validate</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<IResult> ValidateAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        var validationResults = await ValidateCommandAsync(command, cancellationToken);

        if (validationResults.Any())
        {
            return CommandResult.Failure(string.Join(", ", validationResults.Select(x => x.ErrorMessage)));
        }

        return CommandResult.Success();
    }

    /// <summary>
    /// Validates the command
    /// </summary>
    /// <param name="command">The command to validate</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected abstract Task<IEnumerable<ValidationError>> ValidateCommandAsync(
        TCommand command,
        CancellationToken cancellationToken);
}

/// <summary>
/// Represents a validation error with an error message
/// </summary>
public class ValidationError
{
    /// <summary>
    /// Gets the error message
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Initializes a new instance of the ValidationError class
    /// </summary>
    /// <param name="errorMessage">The error message</param>
    public ValidationError(string errorMessage)
    {
        ErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
    }
}