using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.SocialProof.Reviews.Queries.GetProductReviews;

public sealed record GetProductReviewsQuery(
    Guid ProductId,
    int? Rating = null,
    bool? HasMedia = null,
    int Page = 1,
    int PageSize = 10) : IQuery<Result<GetProductReviewsResponse>>;
