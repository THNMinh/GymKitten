using GymKitten.API.Extensions;
using GymKitten.API.Requests;
using GymKitten.Application.Features.Catalog.SizeGuides.Commands.RecommendSize;
using GymKitten.Application.Features.Catalog.SizeGuides.Queries.GetProductSizeGuide;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GymKitten.API.Controllers;

[ApiController]
public class SizeGuideController : ControllerBase
{
    private readonly ISender _sender;

    public SizeGuideController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("api/products/{productId:guid}/size-guide")]
    public async Task<IResult> GetProductSizeGuide(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetProductSizeGuideQuery(productId), cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("api/size-guide/recommend")]
    public async Task<IResult> RecommendSize(
        [FromBody] RecommendSizeRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new RecommendSizeCommand(
            request.ProductId,
            request.HeightCm,
            request.WeightKg,
            request.ChestCm,
            request.WaistCm);

        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
