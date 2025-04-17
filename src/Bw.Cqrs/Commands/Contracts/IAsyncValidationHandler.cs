using Bw.Cqrs.Commands.Results;

namespace Bw.Cqrs.Command.Contract;

/// <summary>
/// Represents an asynchronous validation handler for commands
/// </summary>
/// <typeparam name="T">Type of the object to validate</typeparam>
public interface IAsyncValidationHandler<in T>
{
    /// <summary>
    /// Validates the specified instance asynchronously
    /// </summary>
    /// <param name="instance">Instance to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of validation results</returns>
    Task<IEnumerable<ValidationResult>> ValidateAsync(T instance, CancellationToken cancellationToken);
}
