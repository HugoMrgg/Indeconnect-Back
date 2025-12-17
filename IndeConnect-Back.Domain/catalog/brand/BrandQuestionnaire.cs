namespace IndeConnect_Back.Domain.catalog.brand;
/**
 * Represents a Brand's questionnaire. Able Moderators to see it and accept or refuse the brand. 
 */
public class BrandQuestionnaire
{
    public long Id { get; private set; }
    public long BrandId { get; private set; }
    public Brand Brand { get; private set; } = default!;
    
    public QuestionnaireStatus Status { get; private set; } = QuestionnaireStatus.Draft;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? SubmittedAt { get; private set; }
    public DateTimeOffset? ReviewedAt { get; private set; }
    public long? ReviewerAdminUserId { get; private set; }
    public string? RejectionReason { get; private set; }

    private readonly List<BrandQuestionResponse> _responses = new();
    public IReadOnlyCollection<BrandQuestionResponse> Responses => _responses;

    private BrandQuestionnaire() { }

    public BrandQuestionnaire(long brandId)
    {
        BrandId = brandId;
        CreatedAt = DateTimeOffset.UtcNow;
        Status = QuestionnaireStatus.Draft;
    }
    
    public void MarkSubmitted()
    {
        Status = QuestionnaireStatus.Submitted;
        SubmittedAt = DateTimeOffset.UtcNow;
    }
    public void ReviewApproved(long reviewerAdminUserId)
    {
        Status = QuestionnaireStatus.Approved;
        ReviewerAdminUserId = reviewerAdminUserId;
        ReviewedAt = DateTimeOffset.UtcNow;
        RejectionReason = null;
    }

    public void ReviewRejected(long reviewerAdminUserId, string reason)
    {
        Status = QuestionnaireStatus.Rejected;
        ReviewerAdminUserId = reviewerAdminUserId;
        ReviewedAt = DateTimeOffset.UtcNow;
        RejectionReason = reason?.Trim();
    }
}
