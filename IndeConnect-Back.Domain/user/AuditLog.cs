namespace IndeConnect_Back.Domain.user;

public class AuditLog
{
    public long Id { get; set; }
    public long? UserId { get; set; }        
    public string Action { get; set; } = default!;
    public string? Details { get; set; } 
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
