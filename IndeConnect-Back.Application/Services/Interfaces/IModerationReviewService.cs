using IndeConnect_Back.Application.DTOs.Reviews;
using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IModerationReviewService
{
    Task<IReadOnlyList<ModerationReviewDto>> GetReviewsAsync(
        ReviewStatus? status,
        int page = 1,
        int pageSize = 20,
        string? q = null
    );

    Task ApproveAsync(long reviewId, long moderatorUserId);
    Task RejectAsync(long reviewId, long moderatorUserId);
}