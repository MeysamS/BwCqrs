using System.ComponentModel.Design;
using Bw.Cqrs.Common.Results;

namespace Bw.Cqrs.Command.Contract;

/// <summary>
/// Factory for creating command handlers
/// </summary>
public interface ICommandHandlerFactory
{
    /// <summary>
    /// Creates a command handler for commands without specific result type
    /// </summary>
    /// <typeparam name="TCommand">Type of the command</typeparam>
    /// <returns>Command handler instance</returns>
    ICommandHandler<TCommand> Create<TCommand>() where TCommand : ICommand;

    /// <summary>
    /// Creates a command handler for commands with specific result type
    /// </summary>
    /// <typeparam name="TCommand">Type of the command</typeparam>
    /// <typeparam name="TResult">Type of the result</typeparam>
    /// <returns>Command handler instance</returns>
    ICommandHandler<TCommand, TResult> Create<TCommand, TResult>()
        where TCommand : ICommand
        where TResult : IResult;
}