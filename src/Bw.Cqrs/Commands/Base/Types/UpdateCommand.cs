using Bw.Cqrs.Commands.Base;
using Bw.Cqrs.Common.Results;

namespace Bw.Cqrs.Command.Base.Commands;

/// <summary>
/// Base class for update commands
/// </summary>
/// <typeparam name="TRequest">Type of the entity to update</typeparam>
/// <typeparam name="TResponse">Type of the response</typeparam>
public abstract class UpdateCommand<TRequest, TResponse> : CommandBase
    where TRequest : class
    where TResponse : IResult
{
    /// <summary>
    /// ID of the entity to update
    /// </summary>
    public Guid EntityId { get; }

    /// <summary>
    /// Data for updating the entity
    /// </summary>
    public TRequest Data { get; }

    protected UpdateCommand(Guid entityId, TRequest data)
    {
        if (entityId == Guid.Empty)
            throw new ArgumentException("Entity ID cannot be empty", nameof(entityId));

        EntityId = entityId;
        Data = data ?? throw new ArgumentNullException(nameof(data));
    }
}

/// <summary>
/// Base class for update commands without specific response type
/// </summary>
/// <typeparam name="TRequest">Type of the entity to update</typeparam>
public abstract class UpdateCommand<TRequest> : UpdateCommand<TRequest, IResult>
    where TRequest : class
{
    protected UpdateCommand(Guid entityId, TRequest data) : base(entityId, data)
    {
    }
} 