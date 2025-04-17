using Bw.Cqrs.Common.Results;

namespace Bw.Cqrs.Commands.Contracts;

/// <summary>
/// Delegate for executing the next command handler in the pipeline
/// </summary>
/// <typeparam name="TResult">Type of the result</typeparam>
public delegate Task<TResult> CommandHandlerDelegate<TResult>() where TResult : IResult;

/// <summary>
/// Represents a behavior in the command processing pipeline
/// </summary>
/// <typeparam name="TCommand">Type of the command</typeparam>
/// <typeparam name="TResult">Type of the result</typeparam>
public interface ICommandPipelineBehavior<in TCommand, TResult>
    where TCommand : ICommand
    where TResult : IResult
{
    /// <summary>
    /// Handles the command in the pipeline
    /// </summary>
    /// <param name="command">Command to handle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="next">Delegate to the next handler</param>
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken, CommandHandlerDelegate<TResult> next);
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="TCommand"></typeparam>
public interface ICommandPipelineBehavior<in TCommand> : ICommandPipelineBehavior<TCommand, IResult>
    where TCommand : ICommand
{
} 