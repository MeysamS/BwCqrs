using Bw.Cqrs.Command.Contract;
using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Common.Results;

namespace Bw.Cqrs.Commands.Pipeline.Behaviors;

/// <summary>
/// Middleware for handling command validation using multiple validators
/// </summary>
/// <typeparam name="TCommand">Type of command to validate</typeparam>
public class ValidationBehavior<TCommand> : ICommandPipelineBehavior<TCommand, IResult>
    where TCommand : ICommand
{
    private readonly IValidationHandler<TCommand> _validationHandler;

    public ValidationBehavior(IValidationHandler<TCommand> validationHandler)
    {
        _validationHandler = validationHandler ?? throw new ArgumentNullException(nameof(validationHandler));
    }

    public async Task<IResult> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken,
        CommandHandlerDelegate<IResult> next)
    {
        var validationResult = await _validationHandler.ValidateAsync(command, cancellationToken);
        
        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        return await next();
    }
}

public class ValidationBehavior<TCommand, TResult> : ICommandPipelineBehavior<TCommand, TResult>
    where TCommand : ICommand
    where TResult : class, IResult
{
    private readonly IValidationHandler<TCommand> _validationHandler;

    public ValidationBehavior(IValidationHandler<TCommand> validationHandler)
    {
        _validationHandler = validationHandler ?? throw new ArgumentNullException(nameof(validationHandler));
    }

    public async Task<TResult> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken,
        CommandHandlerDelegate<TResult> next)
    {
        var validationResult = await _validationHandler.ValidateAsync(command, cancellationToken);
        
        if (!validationResult.IsSuccess)
        {
            throw new ValidationException(validationResult.ErrorMessage ?? "Validation failed");
        }

        return await next();
    }
}

public class ValidationException : Exception
{
    public ValidationException(string? error) 
        : base($"Validation failed: {error ?? "No error message provided"}")
    {
    }
} 