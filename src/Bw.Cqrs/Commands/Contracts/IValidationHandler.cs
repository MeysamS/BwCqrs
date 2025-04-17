using Bw.Cqrs.Command.Contract;
using Bw.Cqrs.Common.Results;

namespace Bw.Cqrs.Commands.Contracts;

/// <summary>
/// Represents a validation handler for commands
/// </summary>
/// <typeparam name="TCommand">Type of command to validate</typeparam>
public interface IValidationHandler<in TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// Validates the specified command
    /// </summary>
    /// <param name="command">Command to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    Task<IResult> ValidateAsync(TCommand command, CancellationToken cancellationToken = default);
}

