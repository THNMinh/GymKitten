namespace GymKitten.Application.Features.Admin.Reviews.Queries.GetAdminReviews;

public sealed record GetAdminReviewsResponse(
    List<AdminReviewItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public sealed record AdminReviewItemDto(
    Guid ReviewId,
    Guid ProductId,
    string ProductName,
    Guid UserId,
    string CustomerEmail,
    string CustomerFullName,
    Guid OrderId,
    int Rating,
    string? Comment,
    string? FitFeedback,
    DateTime CreatedAt,
    List<AdminReviewMediaDto> MediaList);

public sealed record AdminReviewMediaDto(
    Guid MediaId,
    string MediaUrl,
    string MediaType);
