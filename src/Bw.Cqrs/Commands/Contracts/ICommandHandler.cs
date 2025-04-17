using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Common.Results;

namespace Bw.Cqrs.Command.Contract;

/// <summary>
/// Represents a handler for a command
/// </summary>
/// <typeparam name="TCommand">The type of command</typeparam>
/// <typeparam name="TResult">The type of result</typeparam>
public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand
    where TResult : IResult
{
    /// <summary>
    /// Handles a command
    /// </summary>
    /// <param name="command">The command</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The result</returns>
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a handler for a command
/// </summary>
/// <typeparam name="TCommand">The type of command</typeparam>
public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    /// <summary>
    /// Handles a command
    /// </summary>
    /// <param name="command">The command</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The result</returns>
    Task<IResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}