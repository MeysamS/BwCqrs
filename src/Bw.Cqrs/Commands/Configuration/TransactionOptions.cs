using System.Data;

namespace Bw.Cqrs.Commands.Configuration;

/// <summary>
/// Options for configuring transaction behavior
/// </summary>
public class TransactionOptions
{
    /// <summary>
    /// Gets or sets the isolation level for transactions
    /// </summary>
    /// <remarks>
    /// Default is ReadCommitted.
    /// </remarks>
    public IsolationLevel IsolationLevel { get; set; } = IsolationLevel.ReadCommitted;
    
    /// <summary>
    /// Gets or sets the timeout for transactions in seconds
    /// </summary>
    /// <remarks>
    /// Default is 30 seconds. Set to 0 for no timeout.
    /// </remarks>
    public int TimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// Gets or sets whether to retry on deadlock exception
    /// </summary>
    /// <remarks>
    /// When true, the transaction will be retried in case of a deadlock exception.
    /// </remarks>
    public bool RetryOnDeadlock { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the maximum number of retry attempts for deadlocks
    /// </summary>
    public int MaxDeadlockRetries { get; set; } = 3;
    
    /// <summary>
    /// Gets or sets the delay in milliseconds between deadlock retry attempts
    /// </summary>
    public int DeadlockRetryDelayMs { get; set; } = 100;
}