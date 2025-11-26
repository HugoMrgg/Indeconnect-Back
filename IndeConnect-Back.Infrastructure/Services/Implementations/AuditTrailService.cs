using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class AuditTrailService : IAuditTrailService
{
    private readonly AppDbContext _db;
    public AuditTrailService(AppDbContext db) => _db = db;

    public async Task LogAsync(string action, long? userId = null, string? details = null)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = action,
            Details = details,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await _db.SaveChangesAsync();
    }
}