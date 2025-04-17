using Bw.Cqrs.Common.Results;

namespace Bw.Cqrs.Command;

/// <summary>
/// Represents a command result
/// </summary>
public sealed class CommandResult : ResultBase
{
    /// <summary>
    /// Initializes a new instance of the CommandResult class
    /// </summary>
    /// <param name="success">A value indicating whether the result is successful</param>
    /// <param name="errorMessage">The error message</param>
    public CommandResult(bool success, string? errorMessage) : base(success, errorMessage)
    {
    }   

    /// <summary>
    /// Gets a success command result
    /// </summary>
    /// <returns>A success command result</returns>
    public static CommandResult Success() => new CommandResult(true, null);

    /// <summary>
    /// Gets a failure command result
    /// </summary>
    /// <param name="errorMessage">The error message</param>
    /// <returns>A failure command result</returns>
    public static CommandResult Failure(string errorMessage) => new CommandResult(false, errorMessage);
}

/// <summary>
/// Represents a command result with a generic type
/// </summary>
/// <typeparam name="T">The type of data</typeparam>
public sealed class CommandResult<T> : ResultBase<T>
{
    /// <summary>
    /// Initializes a new instance of the CommandResult class
    /// </summary>
    public CommandResult(bool success, string? errorMessage, T data) : base(success, errorMessage, data)
    {
    }

    /// <summary>
    /// Gets a success command result
    /// </summary>
    /// <param name="data">The data</param>
    /// <returns>A success command result</returns>
    public static CommandResult<T> Success(T data) => new CommandResult<T>(true, null, data);

    /// <summary>
    /// Gets a failure command result
    /// </summary>
    /// <param name="errorMessage">The error message</param>
    /// <returns>A failure command result</returns>
    public static  CommandResult<T> Failure(string errorMessage) => new CommandResult<T>(false, errorMessage, default!);
}


