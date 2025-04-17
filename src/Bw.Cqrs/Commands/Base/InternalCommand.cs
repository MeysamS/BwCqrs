using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Commands.Enums;

namespace Bw.Cqrs.Commands.Base;

/// <summary>
/// Base class for internal commands that can be scheduled for later execution
/// </summary>
public abstract class InternalCommand : CommandBase, IInternalCommand
{
    /// <inheritdoc />
    public DateTime ScheduledOn { get; private set; }

    /// <inheritdoc />
    public DateTime ExecuteAt { get; private set; }

    /// <inheritdoc />
    public DateTime? ProcessedOn { get; private set; }

    /// <inheritdoc />
    public int RetryCount { get; private set; }

    /// <inheritdoc />
    public string? Error { get; private set; }

    /// <inheritdoc />
    public InternalCommandStatus Status { get; private set; }

    /// <summary>
    /// Initializes a new instance of the InternalCommand class
    /// </summary>
    /// <param name="executeAt">The date and time to execute the command</param> 
    protected InternalCommand(DateTime? executeAt = null)
    {
        ScheduledOn = DateTime.UtcNow;
        ExecuteAt = executeAt ?? ScheduledOn;
        Status = InternalCommandStatus.Scheduled;
        RetryCount = 0;

        if (ExecuteAt < ScheduledOn)
        {
            throw new ArgumentException("Execute time cannot be in the past", nameof(executeAt));
        }
    }

    /// <summary>
    /// Marks the command as being processed
    /// </summary>
    public void MarkAsProcessing()
    {
        Status = InternalCommandStatus.Processing;
    }

    /// <summary>
    /// Marks the command as processed successfully
    /// </summary>
    public void MarkAsProcessed()
    {
        ProcessedOn = DateTime.UtcNow;
        Status = InternalCommandStatus.Processed;
    }

    /// <summary>
    /// Marks the command as failed with the specified error
    /// </summary>
    public void MarkAsFailed(string error)
    {
        Error = error;
        Status = InternalCommandStatus.Failed;
        RetryCount++;
    }

    /// <summary>
    /// Marks the command as cancelled
    /// </summary>
    public void MarkAsCancelled()
    {
        Status = InternalCommandStatus.Cancelled;
    }

    /// <summary>
    /// Determines if the command can be retried
    /// </summary>
    public bool CanRetry(int maxRetries) => 
        Status == InternalCommandStatus.Failed && RetryCount < maxRetries;

    /// <summary>
    /// Determines if the command is ready to be executed
    /// </summary>
    public bool IsReadyToExecute() => 
        Status == InternalCommandStatus.Scheduled && 
        ExecuteAt <= DateTime.UtcNow;
} 