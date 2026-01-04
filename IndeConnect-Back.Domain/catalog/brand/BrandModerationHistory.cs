using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Domain.catalog.brand;

public class BrandModerationHistory
{
    public long Id { get; private set; }
    public long BrandId { get; private set; }
    public Brand Brand { get; private set; } = default!;
    
    public long ModeratorUserId { get; private set; }
    public User ModeratorUser { get; private set; } = default!;
    
    public ModerationAction Action { get; private set; }
    public string? Comment { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // EF Core constructor
    private BrandModerationHistory() { }

    public BrandModerationHistory(
        long brandId, 
        long moderatorUserId, 
        ModerationAction action, 
        string? comment = null)
    {
        BrandId = brandId;
        ModeratorUserId = moderatorUserId;
        Action = action;
        Comment = comment;
        CreatedAt = DateTime.UtcNow;
    }
}

public enum ModerationAction
{
    Submitted = 0,   // SuperVendor soumet
    Approved = 1,    // Moderator approuve
    Rejected = 2     // Moderator rejette
}