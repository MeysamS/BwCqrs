namespace Bw.Cqrs.Common.Results;

/// <summary>
/// Represents a result of an operation
/// </summary>
public interface IResult
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    /// Gets the error message if the operation failed
    /// </summary>
    string? ErrorMessage { get; }
}

/// <summary>
/// Represents a result of an operation with data
/// </summary>
/// <typeparam name="T">Type of the result data</typeparam>
public interface IResult<out T> : IResult
{
    /// <summary>
    /// Gets the result data
    /// </summary>
    T? Data { get; }
}