using Bw.Cqrs.Command.Contract;
using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Common.Results;

namespace Bw.Cqrs.Commands.Pipeline.Behaviors;

/// <summary>
/// Provides validation behavior for command execution pipeline
/// </summary>
/// <typeparam name="TCommand">Type of command</typeparam>
public class ValidationBehavior<TCommand> : ICommandPipelineBehavior<TCommand, IResult>
    where TCommand : ICommand
{
    private readonly IValidationHandler<TCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the validation behavior
    /// </summary>
    /// <param name="validator">Validation handler</param>
    public ValidationBehavior(IValidationHandler<TCommand> validator)
    {
        _validator = validator;
    }

    /// <summary>
    /// Handles the command execution with validation
    /// </summary>
    public async Task<IResult> HandleAsync(TCommand command, CancellationToken cancellationToken, CommandHandlerDelegate<IResult> next)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        return await next();
    }
}

/// <summary>
/// Provides validation behavior for command execution pipeline with specific result type
/// </summary>
public class ValidationBehavior<TCommand, TResult> : ICommandPipelineBehavior<TCommand, TResult>
    where TCommand : ICommand
    where TResult : IResult
{
    private readonly IValidationHandler<TCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the validation behavior
    /// </summary>
    /// <param name="validator">Validation handler</param>
    public ValidationBehavior(IValidationHandler<TCommand> validator)
    {
        _validator = validator;
    }

    /// <summary>
    /// Handles the command execution with validation
    /// </summary>
    public async Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken, CommandHandlerDelegate<TResult> next)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsSuccess)
        {
            throw new ValidationException(validationResult.ErrorMessage);
        }

        return await next();
    }
}

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the validation exception
    /// </summary>
    /// <param name="message">Error message</param>
    public ValidationException(string? message) : base(message)
    {
    }
} 