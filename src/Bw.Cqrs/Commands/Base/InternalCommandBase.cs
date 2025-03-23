using System.Text.Json;
using Bw.Cqrs.Commands.Contracts;

namespace Bw.Cqrs.Commands.Base;

public abstract class InternalCommandBase : CommandBase, IInternalCommand
{
    public DateTime ScheduledOn { get; private set; }
    public DateTime? ProcessedOn { get; private set; }
    public string Type { get; private set; }
    public string Data { get; private set; }

    protected InternalCommandBase()
    {
        ScheduledOn = DateTime.UtcNow;
        Type = GetType().FullName!;
        Data = JsonSerializer.Serialize(this);
    }

    public void MarkAsProcessed()
    {
        ProcessedOn = DateTime.UtcNow;
    }
} 