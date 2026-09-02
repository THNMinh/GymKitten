using GymKitten.API.Extensions;
using GymKitten.Application.Features.Admin.Reviews.Commands.DeleteReview;
using GymKitten.Application.Features.Admin.Reviews.Queries.GetAdminReviews;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymKitten.API.Controllers.Admin;

[ApiController]
[Route("api/admin/reviews")]
[Authorize(Roles = "Admin,admin")]
public class AdminReviewsController : ControllerBase
{
    private readonly ISender _sender;

    public AdminReviewsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IResult> GetAdminReviews(
        [FromQuery] Guid? productId,
        [FromQuery] int? rating,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAdminReviewsQuery(productId, rating, page, pageSize);
        var result = await _sender.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpDelete("{reviewId:guid}")]
    public async Task<IResult> DeleteReview(
        Guid reviewId,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteReviewCommand(reviewId);
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
