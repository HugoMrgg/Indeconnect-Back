namespace IndeConnect_Back.Domain.order;

/// <summary>
/// Représente une étape dans la timeline de suivi d'une livraison.
/// Value object utilisé pour encapsuler le workflow métier de suivi de commande.
/// </summary>
public class DeliveryTrackingStep
{
    public string Status { get; }
    public string Label { get; }
    public string Description { get; }
    public DateTimeOffset? CompletedAt { get; }
    public bool IsCompleted { get; }
    public bool IsCurrent { get; }

    public DeliveryTrackingStep(
        string status,
        string label,
        string description,
        DateTimeOffset? completedAt,
        bool isCompleted,
        bool isCurrent)
    {
        Status = status;
        Label = label;
        Description = description;
        CompletedAt = completedAt;
        IsCompleted = isCompleted;
        IsCurrent = isCurrent;
    }
}
