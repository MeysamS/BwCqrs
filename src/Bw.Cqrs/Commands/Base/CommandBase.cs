using Bw.Cqrs.Commands.Contracts;

namespace Bw.Cqrs.Commands.Base;

public abstract class CommandBase : ICommand
{
    public Guid Id { get; private set; }
    public DateTime Timestamp { get; private set; }

    protected CommandBase()
    {
        Id = Guid.NewGuid();
        Timestamp = DateTime.UtcNow;
    }
} 