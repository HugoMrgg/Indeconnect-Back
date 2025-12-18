namespace IndeConnect_Back.Application.DTOs.Orders;

public class TrackingStepDto
{
    public string Status { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset? CompletedAt { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsCurrent { get; set; }
}