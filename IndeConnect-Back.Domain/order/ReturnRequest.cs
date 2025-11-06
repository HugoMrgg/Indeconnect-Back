namespace IndeConnect_Back.Domain.order;

public class ReturnRequest
{
    public long Id { get; private set; }
    public long OrderId { get; private set; }
    public Order Order { get; private set; } = default!;
    public DateTime RequestedAt { get; private set; }
    public ReturnStatus Status { get; private set; } = ReturnStatus.Requested;
    public string? Reason { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    private ReturnRequest() { }
    public ReturnRequest(long orderId, string? reason)
    {
        OrderId = orderId;
        Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        RequestedAt = DateTime.UtcNow;
    }

    public void MarkApproved(DateTime now)
    {
        Status = ReturnStatus.Approved;
        ProcessedAt = now;
    }
    public void MarkRejected(DateTime now)
    {
        Status = ReturnStatus.Rejected;
        ProcessedAt = now;
    }
}

