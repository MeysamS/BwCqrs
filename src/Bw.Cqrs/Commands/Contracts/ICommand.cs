namespace Bw.Cqrs.Commands.Contracts;

/// <summary>
/// Represents a command in the CQRS system
/// </summary>
public interface ICommand
{
}

/// <summary>
/// Represents a command with request data
/// </summary>
/// <typeparam name="TRequest">Type of the request data</typeparam>
public interface ICommand<TRequest>
    where TRequest : class
{
}