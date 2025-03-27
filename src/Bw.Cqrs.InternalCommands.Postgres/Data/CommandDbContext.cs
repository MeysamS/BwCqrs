using Bw.Cqrs.InternalCommands.Postgres.Models;
using Microsoft.EntityFrameworkCore;

namespace Bw.Cqrs.InternalCommands.Postgres.Data;

public class CommandDbContext : DbContext
{
    public DbSet<CommandEntity> InternalCommands { get; set; } = null!;

    public CommandDbContext(DbContextOptions<CommandDbContext> options)
        : base(options)
    {
    }

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