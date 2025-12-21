using IndeConnect_Back.Application.DTOs.Reviews;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.catalog.product;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class ModerationReviewService : IModerationReviewService
{
    private readonly AppDbContext _context;

    public ModerationReviewService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ModerationReviewDto>> GetReviewsAsync(
        ReviewStatus? status,
        int page = 1,
        int pageSize = 20,
        string? q = null
    )
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize switch
        {
            <= 0 => 20,
            > 100 => 100,
            _ => pageSize
        };

        IQueryable<ProductReview> query = _context.ProductReviews
            .AsNoTracking()
            .Include(r => r.Product)
            .Include(r => r.User);

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            var pattern = $"%{term}%";

            query = query.Where(r =>
                (r.Comment != null && EF.Functions.ILike(r.Comment, pattern)) ||
                EF.Functions.ILike(r.Product.Name, pattern) ||
                EF.Functions.ILike(
                    ((r.User.FirstName ?? "") + " " + (r.User.LastName ?? "")).Trim(),
                    pattern
                )
            );
        }

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ModerationReviewDto(
                r.Id,
                r.ProductId,
                r.Product.Name,
                r.UserId,
                (((r.User.FirstName ?? "") + " " + (r.User.LastName ?? "")).Trim()),
                r.Rating,
                r.Comment,
                r.CreatedAt,
                r.UpdatedAt,
                r.Status
            ))
            .ToListAsync();

        return items;
    }

    public async Task ApproveAsync(long reviewId)
    {
        var review = await _context.ProductReviews.FirstOrDefaultAsync(r => r.Id == reviewId);
        if (review == null)
            throw new KeyNotFoundException("Review not found.");

        review.Approve();
        TouchUpdatedAt(review);

        await _context.SaveChangesAsync();
    }

    public async Task RejectAsync(long reviewId)
    {
        var review = await _context.ProductReviews.FirstOrDefaultAsync(r => r.Id == reviewId);
        if (review == null)
            throw new KeyNotFoundException("Review not found.");

        review.Reject();
        TouchUpdatedAt(review);

        await _context.SaveChangesAsync();
    }

    private void TouchUpdatedAt(ProductReview review)
    {
        // setter privé -> on passe par EF
        _context.Entry(review).Property(nameof(ProductReview.UpdatedAt)).CurrentValue = DateTimeOffset.UtcNow;
    }
}
