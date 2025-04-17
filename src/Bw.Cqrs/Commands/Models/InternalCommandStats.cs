namespace Bw.Cqrs.Commands.Models;

/// <summary>
/// Represents statistics about internal commands in the system
/// </summary>
public class InternalCommandStats
{
    /// <summary>
    /// Gets or sets the total number of commands in the store
    /// </summary>
    public int TotalCommands { get; set; }

    /// <summary>
    /// Gets or sets the number of commands scheduled for execution
    /// </summary>
    public int ScheduledCommands { get; set; }

    /// <summary>
    /// Gets or sets the number of commands currently being processed
    /// </summary>
    public int ProcessingCommands { get; set; }

    /// <summary>
    /// Gets or sets the number of commands that have been processed successfully
    /// </summary>
    public int ProcessedCommands { get; set; }

    /// <summary>
    /// Gets or sets the number of commands that have failed
    /// </summary>
    public int FailedCommands { get; set; }

    /// <summary>
    /// Gets or sets the number of commands that were cancelled
    /// </summary>
    public int CancelledCommands { get; set; }

    /// <summary>
    /// Gets the success rate of command processing
    /// </summary>
    public decimal SuccessRate => TotalCommands > 0 
        ? (decimal)ProcessedCommands / TotalCommands * 100 
        : 0;

    /// <summary>
    /// Gets the failure rate of command processing
    /// </summary>
    public decimal FailureRate => TotalCommands > 0 
        ? (decimal)FailedCommands / TotalCommands * 100 
        : 0;
} 