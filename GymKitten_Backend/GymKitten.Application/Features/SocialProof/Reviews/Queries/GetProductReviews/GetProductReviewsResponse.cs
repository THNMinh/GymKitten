namespace GymKitten.Application.Features.SocialProof.Reviews.Queries.GetProductReviews;

public sealed record GetProductReviewsResponse(
    double AverageRating,
    int TotalReviews,
    RatingBreakdownDto RatingBreakdown,
    FitFeedbackSummaryDto FitFeedbackSummary,
    List<ProductReviewItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public sealed record RatingBreakdownDto(
    int FiveStar,
    int FourStar,
    int ThreeStar,
    int TwoStar,
    int OneStar);

public sealed record FitFeedbackSummaryDto(
    int TrueToSizeCount,
    int RunsSmallCount,
    int RunsLargeCount,
    double TrueToSizePercentage,
    double RunsSmallPercentage,
    double RunsLargePercentage);

public sealed record ProductReviewItemDto(
    Guid ReviewId,
    Guid ProductId,
    Guid UserId,
    string UserFullName,
    string? UserAvatarUrl,
    int Rating,
    string? Comment,
    string? FitFeedback,
    DateTime CreatedAt,
    List<ReviewMediaDto> MediaList);

public sealed record ReviewMediaDto(
    Guid MediaId,
    string MediaUrl,
    string MediaType);
