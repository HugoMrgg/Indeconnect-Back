namespace IndeConnect_Back.Domain.catalog.brand;

public class BrandQuestionnaire
{
    public long Id { get; private set; }
    public long BrandId { get; private set; }
    public Brand Brand { get; private set; } = default!;

    public DateTimeOffset SubmittedAt { get; private set; }
    public bool IsApproved { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }

    private readonly List<BrandQuestionResponse> _responses = new();
    public IReadOnlyCollection<BrandQuestionResponse> Responses => _responses;

    private BrandQuestionnaire() { }

    public BrandQuestionnaire(long brandId, DateTimeOffset submittedAt)
    {
        BrandId = brandId;
        SubmittedAt = submittedAt;
    }

    public void Approve(DateTimeOffset now)
    {
        IsApproved = true;
        ApprovedAt = now;
    }
}
