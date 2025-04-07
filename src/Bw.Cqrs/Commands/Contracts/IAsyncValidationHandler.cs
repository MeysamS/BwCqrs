using Bw.Cqrs.Commands.Results;

namespace Bw.Cqrs.Commands.Contracts;

public interface IAsyncValidationHandler<in T>
{
    Task<IEnumerable<ValidationResult>> ValidateAsync(T instance, CancellationToken cancellationToken);
}
