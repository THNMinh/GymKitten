using GymKitten.Domain.Entities;

namespace GymKitten.Application.Abstractions.Repositories;

public interface IReviewRepository
{
    Task<Productreview?> GetByIdAsync(Guid reviewId, CancellationToken cancellationToken = default);

    Task<Productreview?> GetByIdWithMediaAsync(Guid reviewId, CancellationToken cancellationToken = default);

    Task<bool> UserPurchasedProductAsync(Guid userId, Guid productId, Guid orderId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByOrderAndProductAsync(Guid orderId, Guid productId, CancellationToken cancellationToken = default);

    Task<(IEnumerable<Productreview> Items, int TotalCount, double AverageRating, RatingBreakdownData RatingBreakdown, FitFeedbackData FitFeedbackSummary)> SearchProductReviewsAsync(
        Guid productId,
        int? rating,
        bool? hasMedia,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<(IEnumerable<Productreview> Items, int TotalCount)> SearchAdminReviewsAsync(
        Guid? productId,
        int? rating,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task AddAsync(Productreview review, CancellationToken cancellationToken = default);

    Task AddMediaAsync(List<Reviewmedia> mediaList, CancellationToken cancellationToken = default);

    void Remove(Productreview review);
}

public sealed record RatingBreakdownData(
    int FiveStar,
    int FourStar,
    int ThreeStar,
    int TwoStar,
    int OneStar);

public sealed record FitFeedbackData(
    int TrueToSizeCount,
    int RunsSmallCount,
    int RunsLargeCount);
