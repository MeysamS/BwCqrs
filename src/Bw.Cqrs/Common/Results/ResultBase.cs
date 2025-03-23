namespace Bw.Cqrs.Common.Results;

public abstract class ResultBase : IResult
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }

    protected ResultBase(bool success, string? errorMessage)
    {
        IsSuccess = success;
        ErrorMessage = errorMessage;
    }
}

public abstract class ResultBase<T> : ResultBase, IResult<T>
{
    protected ResultBase(bool success, string? errorMessage, T data) : base(success, errorMessage)
    {
        Data = data;
    }
    public T Data { get; }
}