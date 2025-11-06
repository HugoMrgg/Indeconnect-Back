using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Domain.order;
/**
 * Represents a User's Return Request
 */
public class ReturnRequest
{
    public long Id { get; private set; }
    public long OrderId { get; private set; }
    public Order Order { get; private set; } = default!;
    
    public long UserId { get; private set; }
    public User User { get; private set; } = default!;
    
    public DateTimeOffset RequestedAt { get; private set; }
    public ReturnStatus Status { get; private set; } = ReturnStatus.Requested;
    public string? Reason { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }

    private ReturnRequest() { }
    
    public ReturnRequest(long orderId, long userId, string? reason)
    {
        OrderId = orderId;
        UserId = userId;
        Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        RequestedAt = DateTimeOffset.UtcNow;
        Status = ReturnStatus.Requested;
    }
    
    public void Approve()
    {
        if (Status != ReturnStatus.Requested)
            throw new InvalidOperationException("Only requested returns can be approved");
            
        Status = ReturnStatus.Approved;
        ProcessedAt = DateTimeOffset.UtcNow;
    }
    
    public void Reject()
    {
        if (Status != ReturnStatus.Requested)
            throw new InvalidOperationException("Only requested returns can be rejected");
            
        Status = ReturnStatus.Rejected;
        ProcessedAt = DateTimeOffset.UtcNow;
    }
}
