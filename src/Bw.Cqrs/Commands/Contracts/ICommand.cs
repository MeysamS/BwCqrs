namespace Bw.Cqrs.Commands.Contracts;

public interface ICommand
{
}

public interface ICommand<TRequest>
    where TRequest : class
{}