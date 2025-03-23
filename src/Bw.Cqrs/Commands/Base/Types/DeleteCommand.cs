using Bw.Cqrs.Commands.Base;
using Bw.Cqrs.Common.Results;

namespace Bw.Cqrs.Command.Base.Commands;

/// <summary>
/// Base class for delete commands
/// </summary>
/// <typeparam name="TResponse">Type of the response</typeparam>
public abstract class DeleteCommand<TResponse> : CommandBase
    where TResponse : IResult
{
    /// <summary>
    /// ID of the entity to delete
    /// </summary>
    public Guid EntityId { get; }

    protected DeleteCommand(Guid entityId)
    {
        if (entityId == Guid.Empty)
            throw new ArgumentException("Entity ID cannot be empty", nameof(entityId));

        EntityId = entityId;
    }
}

/// <summary>
/// Base class for delete commands without specific response type
/// </summary>
public abstract class DeleteCommand : DeleteCommand<IResult>
{
    protected DeleteCommand(Guid entityId) : base(entityId)
    {
    }
} 