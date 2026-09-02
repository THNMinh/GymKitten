using GymKitten.API.Extensions;
using GymKitten.API.Requests;
using GymKitten.Application.Features.SocialProof.Reviews.Commands.CreateReview;
using GymKitten.Application.Features.SocialProof.Reviews.Commands.UploadReviewMedia;
using GymKitten.Application.Features.SocialProof.Reviews.Queries.GetProductReviews;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymKitten.API.Controllers;

[ApiController]
public class ReviewsController : ControllerBase
{
    private readonly ISender _sender;

    public ReviewsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("api/products/{productId:guid}/reviews")]
    public async Task<IResult> GetProductReviews(
        Guid productId,
        [FromQuery] int? rating,
        [FromQuery] bool? hasMedia,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductReviewsQuery(productId, rating, hasMedia, page, pageSize);
        var result = await _sender.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("api/reviews")]
    [Authorize]
    public async Task<IResult> CreateReview(
        [FromBody] CreateReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateReviewCommand(
            request.ProductId,
            request.OrderId,
            request.Rating,
            request.Comment,
            request.FitFeedback);

        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("api/reviews/{reviewId:guid}/media")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IResult> UploadReviewMedia(
        Guid reviewId,
        [FromForm] List<IFormFile> files,
        CancellationToken cancellationToken = default)
    {
        var command = new UploadReviewMediaCommand(reviewId, files);
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
