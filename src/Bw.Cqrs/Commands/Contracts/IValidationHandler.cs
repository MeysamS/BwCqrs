using Bw.Cqrs.Common.Results;

namespace Bw.Cqrs.Commands.Contracts;

public interface IValidationHandler<in TCommand>
    where TCommand : ICommand
{
    Task<IResult> ValidateAsync(TCommand command, CancellationToken cancellationToken = default);
}

