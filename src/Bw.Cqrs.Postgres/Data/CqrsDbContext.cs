using Bw.Cqrs.Postgres.Models;
using Microsoft.EntityFrameworkCore;

namespace Bw.Cqrs.Postgres.Data;

public class CqrsDbContext : DbContext
{
    public DbSet<InternalCommandEntry> InternalCommands { get; set; } = default!;

    public CqrsDbContext(DbContextOptions<CqrsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InternalCommandEntry>(builder =>
        {
            builder.ToTable("internal_commands");
            
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.Type)
                .IsRequired()
                .HasMaxLength(500);
            
            builder.Property(x => x.Data)
                .IsRequired();
            
            builder.Property(x => x.ScheduledOn)
                .IsRequired();
            
            builder.Property(x => x.Error)
                .HasMaxLength(2000);
        });
    }
} 