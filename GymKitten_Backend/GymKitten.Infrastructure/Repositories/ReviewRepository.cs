using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymKitten.Infrastructure.Repositories;

public sealed class ReviewRepository : IReviewRepository
{
    private readonly GymkittenContext _context;

    public ReviewRepository(GymkittenContext context)
    {
        _context = context;
    }

    public async Task<Productreview?> GetByIdAsync(Guid reviewId, CancellationToken cancellationToken = default)
    {
        return await _context.Productreviews
            .FirstOrDefaultAsync(r => r.Reviewid == reviewId, cancellationToken);
    }

    public async Task<Productreview?> GetByIdWithMediaAsync(Guid reviewId, CancellationToken cancellationToken = default)
    {
        return await _context.Productreviews
            .Include(r => r.Reviewmedia)
            .FirstOrDefaultAsync(r => r.Reviewid == reviewId, cancellationToken);
    }

    public async Task<bool> UserPurchasedProductAsync(Guid userId, Guid productId, Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .AsNoTracking()
            .Where(o => o.Orderid == orderId && o.Userid == userId)
            .AnyAsync(o => o.Orderitems.Any(item => item.Variant.Productid == productId || item.Variantid == productId), cancellationToken);
    }

    public async Task<bool> ExistsByOrderAndProductAsync(Guid orderId, Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.Productreviews
            .AsNoTracking()
            .AnyAsync(r => r.Orderid == orderId && r.Productid == productId, cancellationToken);
    }

    public async Task<(List<Productreview> Items, int TotalCount, double AverageRating, RatingBreakdownData RatingBreakdown, FitFeedbackData FitFeedbackSummary)> GetProductReviewsPagedAsync(
        Guid productId,
        int? rating,
        bool? hasMedia,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var allProductReviewsQuery = _context.Productreviews
            .AsNoTracking()
            .Where(r => r.Productid == productId);

        var totalReviewsCount = await allProductReviewsQuery.CountAsync(cancellationToken);

        double avgRating = 0;
        int fiveStar = 0, fourStar = 0, threeStar = 0, twoStar = 0, oneStar = 0;
        int trueToSize = 0, runsSmall = 0, runsLarge = 0;

        if (totalReviewsCount > 0)
        {
            avgRating = await allProductReviewsQuery.AverageAsync(r => r.Rating, cancellationToken);
            avgRating = Math.Round(avgRating, 1);

            fiveStar = await allProductReviewsQuery.CountAsync(r => r.Rating == 5, cancellationToken);
            fourStar = await allProductReviewsQuery.CountAsync(r => r.Rating == 4, cancellationToken);
            threeStar = await allProductReviewsQuery.CountAsync(r => r.Rating == 3, cancellationToken);
            twoStar = await allProductReviewsQuery.CountAsync(r => r.Rating == 2, cancellationToken);
            oneStar = await allProductReviewsQuery.CountAsync(r => r.Rating == 1, cancellationToken);

            trueToSize = await allProductReviewsQuery.CountAsync(r => r.Fitfeedback != null && r.Fitfeedback.ToLower() == "truetosize", cancellationToken);
            runsSmall = await allProductReviewsQuery.CountAsync(r => r.Fitfeedback != null && r.Fitfeedback.ToLower() == "runssmall", cancellationToken);
            runsLarge = await allProductReviewsQuery.CountAsync(r => r.Fitfeedback != null && r.Fitfeedback.ToLower() == "runslarge", cancellationToken);
        }

        var filteredQuery = allProductReviewsQuery
            .Include(r => r.User)
            .Include(r => r.Reviewmedia)
            .AsQueryable();

        if (rating.HasValue && rating.Value >= 1 && rating.Value <= 5)
        {
            filteredQuery = filteredQuery.Where(r => r.Rating == rating.Value);
        }

        if (hasMedia.HasValue)
        {
            if (hasMedia.Value)
            {
                filteredQuery = filteredQuery.Where(r => r.Reviewmedia.Any());
            }
            else
            {
                filteredQuery = filteredQuery.Where(r => !r.Reviewmedia.Any());
            }
        }

        var totalFilteredCount = await filteredQuery.CountAsync(cancellationToken);

        var items = await filteredQuery
            .OrderByDescending(r => r.Createdat)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var ratingBreakdown = new RatingBreakdownData(fiveStar, fourStar, threeStar, twoStar, oneStar);
        var fitFeedbackSummary = new FitFeedbackData(trueToSize, runsSmall, runsLarge);

        return (items, totalFilteredCount, avgRating, ratingBreakdown, fitFeedbackSummary);
    }

    public async Task<(List<Productreview> Items, int TotalCount)> GetAdminReviewsPagedAsync(
        Guid? productId,
        int? rating,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Productreviews
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Product)
            .Include(r => r.Reviewmedia)
            .AsQueryable();

        if (productId.HasValue && productId.Value != Guid.Empty)
        {
            query = query.Where(r => r.Productid == productId.Value);
        }

        if (rating.HasValue && rating.Value >= 1 && rating.Value <= 5)
        {
            query = query.Where(r => r.Rating == rating.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.Createdat)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Productreview review, CancellationToken cancellationToken = default)
    {
        await _context.Productreviews.AddAsync(review, cancellationToken);
    }

    public async Task AddMediaAsync(List<Reviewmedia> mediaList, CancellationToken cancellationToken = default)
    {
        await _context.Reviewmedias.AddRangeAsync(mediaList, cancellationToken);
    }

    public void Remove(Productreview review)
    {
        _context.Productreviews.Remove(review);
    }
}
