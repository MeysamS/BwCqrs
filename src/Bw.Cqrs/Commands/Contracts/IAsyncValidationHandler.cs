using Bw.Cqrs.Commands.Results;

namespace Bw.Cqrs.Command.Contract;

public interface IAsyncValidationHandler<in T>
{
    Task<IEnumerable<ValidationResult>> ValidateAsync(T instance, CancellationToken cancellationToken);
}
