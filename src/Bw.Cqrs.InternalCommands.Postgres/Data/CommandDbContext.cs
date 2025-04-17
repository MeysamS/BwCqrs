using Bw.Cqrs.InternalCommands.Postgres.Models;
using Microsoft.EntityFrameworkCore;

namespace Bw.Cqrs.InternalCommands.Postgres.Data;

/// <summary>
/// Database context for internal command storage
/// </summary>
public class CommandDbContext : DbContext
{
    /// <summary>
    /// Gets or sets the internal commands DbSet
    /// </summary>
    public DbSet<CommandEntity> InternalCommands { get; set; } = null!;

    /// <summary>
    /// Initializes a new instance of the command database context
    /// </summary>
    /// <param name="options">The options for configuring the context</param>
    public CommandDbContext(DbContextOptions<CommandDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Configures the database model
    /// </summary>
    /// <param name="modelBuilder">The model builder</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CommandEntity>(entity =>
        {
            entity.ToTable("internal_commands");
            
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ScheduledOn);
            entity.HasIndex(e => e.ProcessedOn);
        });
    }
} 