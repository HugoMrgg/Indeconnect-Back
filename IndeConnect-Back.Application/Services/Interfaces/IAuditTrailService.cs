namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IAuditTrailService
{
    Task LogAsync(string action, long? userId = null, string? details = null);
}
