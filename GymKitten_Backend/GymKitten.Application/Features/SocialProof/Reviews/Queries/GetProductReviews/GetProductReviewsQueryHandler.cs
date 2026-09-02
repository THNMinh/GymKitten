using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.SocialProof.Reviews.Queries.GetProductReviews;

public sealed class GetProductReviewsQueryHandler
    : IQueryHandler<GetProductReviewsQuery, Result<GetProductReviewsResponse>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IProductRepository _productRepository;

    public GetProductReviewsQueryHandler(
        IReviewRepository reviewRepository,
        IProductRepository productRepository)
    {
        _reviewRepository = reviewRepository;
        _productRepository = productRepository;
    }

    public async Task<Result<GetProductReviewsResponse>> Handle(
        GetProductReviewsQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure<GetProductReviewsResponse>(ProductErrors.NotFound);
        }

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        var (items, totalCount, avgRating, ratingBreakdown, fitFeedback) =
            await _reviewRepository.SearchProductReviewsAsync(
                request.ProductId,
                request.Rating,
                request.HasMedia,
                page,
                pageSize,
                cancellationToken);

        var totalFitFeedbacks = fitFeedback.TrueToSizeCount + fitFeedback.RunsSmallCount + fitFeedback.RunsLargeCount;
        double trueToSizePct = totalFitFeedbacks > 0 ? Math.Round((double)fitFeedback.TrueToSizeCount / totalFitFeedbacks * 100, 1) : 0;
        double runsSmallPct = totalFitFeedbacks > 0 ? Math.Round((double)fitFeedback.RunsSmallCount / totalFitFeedbacks * 100, 1) : 0;
        double runsLargePct = totalFitFeedbacks > 0 ? Math.Round((double)fitFeedback.RunsLargeCount / totalFitFeedbacks * 100, 1) : 0;

        var reviewDtos = items.Select(r => new ProductReviewItemDto(
            r.Reviewid,
            r.Productid,
            r.Userid,
            (r.User != null && !string.IsNullOrEmpty(r.User.Fullname)) ? r.User.Fullname : "Customer",
            r.User?.Avatarurl,
            r.Rating,
            r.Comment,
            r.Fitfeedback,
            r.Createdat,
            r.Reviewmedia.Select(m => new ReviewMediaDto(m.Mediaid, m.Mediaurl, m.Mediatype)).ToList()
        )).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var response = new GetProductReviewsResponse(
            avgRating,
            totalCount,
            new RatingBreakdownDto(
                ratingBreakdown.FiveStar,
                ratingBreakdown.FourStar,
                ratingBreakdown.ThreeStar,
                ratingBreakdown.TwoStar,
                ratingBreakdown.OneStar),
            new FitFeedbackSummaryDto(
                fitFeedback.TrueToSizeCount,
                fitFeedback.RunsSmallCount,
                fitFeedback.RunsLargeCount,
                trueToSizePct,
                runsSmallPct,
                runsLargePct),
            reviewDtos,
            totalCount,
            page,
            pageSize,
            totalPages);

        return Result.Success(response);
    }
}
