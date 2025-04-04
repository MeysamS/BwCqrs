namespace Bw.Cqrs.Commands.Configuration;

/// <summary>
/// Configuration options for internal commands
/// </summary>
public class InternalCommandOptions
{
    /// <summary>
    /// Gets or sets the maximum number of retry attempts for failed commands
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Gets or sets the delay between retry attempts in seconds
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets the interval for processing commands in seconds
    /// </summary>
    public int ProcessingIntervalSeconds { get; set; } = 10;

    /// <summary>
    /// Gets or sets the number of days to keep processed commands before cleanup
    /// </summary>
    public int RetentionDays { get; set; } = 7;
} 