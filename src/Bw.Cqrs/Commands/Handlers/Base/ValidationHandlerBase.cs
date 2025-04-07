using Bw.Cqrs.Command;
using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Common.Results;

namespace Bw.Cqrs.Commands.Base;

public abstract class ValidationHandlerBase<TCommand> : IValidationHandler<TCommand>
    where TCommand : ICommand
{
    public virtual async Task<IResult> ValidateAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        var validationResults = await ValidateCommandAsync(command, cancellationToken);
        
        if (validationResults.Any())
        {
            return CommandResult.Failure(string.Join(", ", validationResults.Select(x => x.ErrorMessage)));
        }

        return CommandResult.Success();
    }

    protected abstract Task<IEnumerable<ValidationError>> ValidateCommandAsync(
        TCommand command, 
        CancellationToken cancellationToken);
}

public class ValidationError
{
    public string ErrorMessage { get; }

    public ValidationError(string errorMessage)
    {
        ErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
    }
} 