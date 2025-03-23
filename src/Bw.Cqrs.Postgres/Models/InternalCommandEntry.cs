using System.Text.Json;
using Bw.Cqrs.Commands.Base;
using Bw.Cqrs.Commands.Contracts;

namespace Bw.Cqrs.Postgres.Models;

public class InternalCommandEntry
{
    public Guid Id { get; set; }
    public string Type { get; set; } = default!;
    public string Data { get; set; } = default!;
    public DateTime ScheduledOn { get; set; }
    public DateTime? ProcessedOn { get; set; }
    public string? Error { get; set; }

    public static InternalCommandEntry FromCommand(IInternalCommand command)
    {
        return new InternalCommandEntry
        {
            Id = ((CommandBase)command).Id,
            Type = command.GetType().FullName!,
            Data = JsonSerializer.Serialize(command),
            ScheduledOn = command.ScheduledOn
        };
    }
} 