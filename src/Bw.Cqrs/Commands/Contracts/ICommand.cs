namespace Bw.Cqrs.Command.Contract;

public interface ICommand
{
}

public interface ICommand<TRequest>
    where TRequest : class
{}