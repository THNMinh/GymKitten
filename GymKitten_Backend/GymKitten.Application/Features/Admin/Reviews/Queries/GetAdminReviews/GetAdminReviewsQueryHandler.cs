using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Admin.Reviews.Queries.GetAdminReviews;

public sealed class GetAdminReviewsQueryHandler
    : IQueryHandler<GetAdminReviewsQuery, Result<GetAdminReviewsResponse>>
{
    private readonly IUserContext _userContext;
    private readonly IReviewRepository _reviewRepository;

    public GetAdminReviewsQueryHandler(
        IUserContext userContext,
        IReviewRepository reviewRepository)
    {
        _userContext = userContext;
        _reviewRepository = reviewRepository;
    }

    public async Task<Result<GetAdminReviewsResponse>> Handle(
        GetAdminReviewsQuery request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(_userContext.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<GetAdminReviewsResponse>(UserErrors.Forbidden);
        }

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        var (items, totalCount) = await _reviewRepository.SearchAdminReviewsAsync(
            request.ProductId,
            request.Rating,
            page,
            pageSize,
            cancellationToken);

        var dtos = items.Select(r => new AdminReviewItemDto(
            r.Reviewid,
            r.Productid,
            r.Product != null ? r.Product.Name : "Product",
            r.Userid,
            r.User != null ? r.User.Email : "Unknown",
            (r.User != null && !string.IsNullOrEmpty(r.User.Fullname)) ? r.User.Fullname : "Customer",
            r.Orderid,
            r.Rating,
            r.Comment,
            r.Fitfeedback,
            r.Createdat,
            r.Reviewmedia.Select(m => new AdminReviewMediaDto(m.Mediaid, m.Mediaurl, m.Mediatype)).ToList()
        )).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var response = new GetAdminReviewsResponse(
            dtos,
            totalCount,
            page,
            pageSize,
            totalPages);

        return Result.Success(response);
    }
}
