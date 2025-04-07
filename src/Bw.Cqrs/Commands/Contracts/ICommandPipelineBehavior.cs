using Bw.Cqrs.Common.Results;

namespace Bw.Cqrs.Commands.Contracts;

public delegate Task<TResult> CommandHandlerDelegate<TResult>() where TResult : IResult;

public interface ICommandPipelineBehavior<in TCommand, TResult>
    where TCommand : ICommand
    where TResult : IResult
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken, CommandHandlerDelegate<TResult> next);
}

public interface ICommandPipelineBehavior<in TCommand> : ICommandPipelineBehavior<TCommand, IResult>
    where TCommand : ICommand
{
} 