using Bw.Cqrs.Common.Results;

namespace Bw.Cqrs.Commands.Contracts;

/// <summary>
/// Represents a command bus for dispatching commands
/// </summary>
public interface ICommandBus
{
    /// <summary>
    /// Dispatches a command asynchronously
    /// </summary>
    /// <typeparam name="TCommand">Type of the command</typeparam>
    /// <param name="command">Command to dispatch</param>
    Task DispatchAsync<TCommand>(TCommand command) where TCommand : ICommand;

    /// <summary>
    /// Dispatches a command asynchronously and returns a result
    /// </summary>
    /// <typeparam name="TCommand">Type of the command</typeparam>
    /// <typeparam name="TResult">Type of the result</typeparam>
    /// <param name="command">Command to dispatch</param>
    Task<TResult?> DispatchAsync<TCommand, TResult>(TCommand command)
        where TCommand : ICommand
        where TResult : class, IResult;

    /// <summary>
    /// Dispatches a command synchronously
    /// </summary>
    /// <typeparam name="TCommand">Type of the command</typeparam>
    /// <param name="command">Command to dispatch</param>
    void Dispatch<TCommand>(TCommand command) where TCommand : ICommand;

    /// <summary>
    /// Schedules a command for later execution
    /// </summary>
    /// <typeparam name="TCommand">Type of the command</typeparam>
    /// <param name="command">Command to schedule</param>
    Task ScheduleAsync<TCommand>(TCommand command) 
        where TCommand : IInternalCommand;
}