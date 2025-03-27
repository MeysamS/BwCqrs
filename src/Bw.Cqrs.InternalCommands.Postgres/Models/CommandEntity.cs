using System.ComponentModel.DataAnnotations;
using Bw.Cqrs.Commands.Enums;

namespace Bw.Cqrs.InternalCommands.Postgres.Models;

public class CommandEntity
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Type { get; set; } = string.Empty;
    
    [Required]
    public string Data { get; set; } = string.Empty;
    
    [Required]
    public DateTime ScheduledOn { get; set; }
    
    public DateTime? ProcessedOn { get; set; }
    
    public int RetryCount { get; set; }
    
    [MaxLength(2000)]
    public string? Error { get; set; }
    
    [Required]
    public InternalCommandStatus Status { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? LastRetryAt { get; set; }
} 