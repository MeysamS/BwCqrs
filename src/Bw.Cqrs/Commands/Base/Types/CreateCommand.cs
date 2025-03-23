using Bw.Cqrs.Commands.Base;
using Bw.Cqrs.Common.Results;

namespace Bw.Cqrs.Command.Base.Commands;

/// <summary>
/// Base class for create commands
/// </summary>
/// <typeparam name="TRequest">Type of the entity to create</typeparam>
/// <typeparam name="TResponse">Type of the response</typeparam>
public abstract class CreateCommand<TRequest, TResponse> : CommandBase
    where TRequest : class
    where TResponse : IResult
{
    /// <summary>
    /// Data for creating the entity
    /// </summary>
    public TRequest Data { get; }

    protected CreateCommand(TRequest data)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
    }
}

/// <summary>
/// Base class for create commands without specific response type
/// </summary>
/// <typeparam name="TRequest">Type of the entity to create</typeparam>
public abstract class CreateCommand<TRequest> : CreateCommand<TRequest, IResult>
    where TRequest : class
{
    protected CreateCommand(TRequest data) : base(data)
    {
    }
} 