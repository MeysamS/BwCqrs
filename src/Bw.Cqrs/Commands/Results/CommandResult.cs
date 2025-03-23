using Bw.Cqrs.Common.Results;

namespace Bw.Cqrs.Command;

public sealed class CommandResult : ResultBase
{
    public CommandResult(bool success, string? errorMessage) : base(success, errorMessage)
    {
    }
    public  static CommandResult Success() => new CommandResult(true, null);
    public static CommandResult Failure(string errorMessage) => new CommandResult(false, errorMessage);
}

public sealed class CommandResult<T> : ResultBase<T>
{
    public CommandResult(bool success, string? errorMessage, T data) : base(success, errorMessage, data)
    {
    }
    public  static CommandResult<T> Success(T data) => new CommandResult<T>(true, null, data);
    public static  CommandResult<T> Failure(string errorMessage) => new CommandResult<T>(false, errorMessage, default!);
}


