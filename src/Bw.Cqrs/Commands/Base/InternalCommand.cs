using Bw.Cqrs.Commands.Contracts;
using Bw.Cqrs.Commands.Enums;
using Newtonsoft.Json;

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
    /// Internal command constructor.
    /// </summary>
    protected InternalCommand() : base()
    {
        ScheduledOn = DateTime.UtcNow;
        ExecuteAt = ScheduledOn;
        Status = InternalCommandStatus.Scheduled;
        RetryCount = 0;
    }

    /// <summary>
    /// Internal command constructor.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="timestamp"></param>
    /// <param name="scheduledOn"></param>
    /// <param name="executeAt"></param>
    /// <param name="processedOn"></param>
    /// <param name="retryCount"></param>
    /// <param name="error"></param>
    /// <param name="status"></param>
    [JsonConstructor]
    protected InternalCommand(Guid id, DateTime timestamp,
                              DateTime scheduledOn, DateTime executeAt,
                              DateTime? processedOn, int retryCount,
                              string? error, InternalCommandStatus status)
        : base(id, timestamp)
    {
        ScheduledOn = scheduledOn;
        ExecuteAt = executeAt;
        ProcessedOn = processedOn;
        RetryCount = retryCount;
        Error = error;
        Status = status;
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