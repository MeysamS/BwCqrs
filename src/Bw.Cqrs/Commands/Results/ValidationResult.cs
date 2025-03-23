namespace Bw.Cqrs.Commands.Results;

public class ValidationResult
{
    public string Message { get; }
    public ValidationResult(string message)
    {
        Message = message;
    }
}
