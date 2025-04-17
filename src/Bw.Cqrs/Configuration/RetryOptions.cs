namespace Bw.Cqrs.Configuration;

/// <summary>
/// Represents the options for retry behavior
/// </summary>
public class RetryOptions
{
    /// <summary>
    /// Gets or sets the maximum number of retries
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Gets or sets the delay in milliseconds between retries
    /// </summary>
    public int DelayMilliseconds { get; set; } = 1000;
} 