namespace Bw.Cqrs.InternalCommands.Postgres.Models;

/// <summary>
/// Postgres options
/// </summary>
public class PostgresOptions
{
    /// <summary>
    /// Postgres connection string
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
    /// <summary>
    /// Enable detailed errors
    /// </summary>
    public bool EnableDetailedErrors { get; set; }
    /// <summary>
    /// Enable sensitive data logging
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; }
    /// <summary>
    /// Command timeout
    /// </summary>
    public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromSeconds(30);
    /// <summary>
    /// Validate options
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public void Validate()
    {
        if (string.IsNullOrEmpty(ConnectionString))
            throw new InvalidOperationException("Connection string cannot be empty");

        if (CommandTimeout <= TimeSpan.Zero)
            throw new InvalidOperationException("Command timeout must be greater than zero");
    }
}