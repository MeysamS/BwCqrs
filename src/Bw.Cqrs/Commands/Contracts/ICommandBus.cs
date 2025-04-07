using Bw.Cqrs.Common.Results;

namespace Bw.Cqrs.Commands.Contracts;

public interface ICommandBus
{
    Task DispatchAsync<TCommand>(TCommand command) where TCommand : ICommand;

    Task<TResult?> DispatchAsync<TCommand, TResult>(TCommand command)
        where TCommand : ICommand
        where TResult : class, IResult;

    void Dispatch<TCommand>(TCommand command) where TCommand : ICommand;


    Task ScheduleAsync<TCommand>(TCommand command) 
        where TCommand : IInternalCommand;
}