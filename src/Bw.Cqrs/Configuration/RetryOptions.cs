namespace Bw.Cqrs.Configuration;

public class RetryOptions
{
    public int MaxRetries { get; set; } = 3;
    public int DelayMilliseconds { get; set; } = 1000;
} 