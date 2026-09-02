using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Admin.Reviews.Queries.GetAdminReviews;

public sealed record GetAdminReviewsQuery(
    Guid? ProductId = null,
    int? Rating = null,
    int Page = 1,
    int PageSize = 10) : IQuery<Result<GetAdminReviewsResponse>>;
