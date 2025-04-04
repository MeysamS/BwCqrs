namespace Bw.Cqrs.InternalCommands.Postgres.Models;

public class PostgresOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public bool EnableDetailedErrors { get; set; }
    public bool EnableSensitiveDataLogging { get; set; }
    public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromSeconds(30);

    public void Validate()
    {
        if (string.IsNullOrEmpty(ConnectionString))
            throw new InvalidOperationException("Connection string cannot be empty");

        if (CommandTimeout <= TimeSpan.Zero)
            throw new InvalidOperationException("Command timeout must be greater than zero");
    }
} 