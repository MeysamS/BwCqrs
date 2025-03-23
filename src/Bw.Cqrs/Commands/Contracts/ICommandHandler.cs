using Bw.Cqrs.Common.Results;

namespace Bw.Cqrs.Command.Contract;

public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand
    where TResult : IResult
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    Task<IResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}