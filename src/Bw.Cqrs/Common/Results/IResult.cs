namespace Bw.Cqrs.Common.Results;

public interface IResult
{
    bool IsSuccess { get; }
    string? ErrorMessage { get; }
}

public interface IResult<out T> : IResult
{
    T Data { get; }
}