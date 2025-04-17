namespace Bw.Cqrs.Commands.Enums;

/// <summary>
/// Represents the status of an internal command
/// </summary>
public enum InternalCommandStatus
{
    /// <summary>
    /// Command is scheduled but not yet processed
    /// </summary>
    Scheduled = 0,

    /// <summary>
    /// Command is currently being processed
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Command was processed successfully
    /// </summary>
    Processed = 2,

    /// <summary>
    /// Command processing failed
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Command was cancelled
    /// </summary>
    Cancelled = 4
} 