namespace Bw.Cqrs.Common.Results;

/// <summary>
/// Represents a base class for results
/// </summary>
public abstract class ResultBase : IResult
{
    /// <summary>
    /// Gets a value indicating whether the result is successful
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the error message
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Initializes a new instance of the ResultBase class
    /// </summary>
    /// <param name="success">A value indicating whether the result is successful</param>
    /// <param name="errorMessage">The error message</param>
    protected ResultBase(bool success, string? errorMessage)
    {
        IsSuccess = success;
        ErrorMessage = errorMessage;
    }
}

/// <summary>
/// Represents a base class for results with a generic type
/// </summary>
/// <typeparam name="T">The type of data</typeparam>
public abstract class ResultBase<T> : ResultBase, IResult<T>
{
    /// <summary>
    /// Initializes a new instance of the ResultBase class
    /// </summary>
    /// <param name="success">A value indicating whether the result is successful</param>
    /// <param name="errorMessage">The error message</param>
    /// <param name="data">The data</param>
    protected ResultBase(bool success, string? errorMessage, T data) : base(success, errorMessage)
    {
        Data = data;
    }

    /// <summary>
    /// Gets the data
    /// </summary>
    public T Data { get; }
}