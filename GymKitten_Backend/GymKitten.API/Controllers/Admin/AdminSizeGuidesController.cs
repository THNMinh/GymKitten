using GymKitten.API.Extensions;
using GymKitten.API.Requests;
using GymKitten.Application.Features.Admin.SizeGuides.Commands.CreateSizeGuide;
using GymKitten.Application.Features.Admin.SizeGuides.Commands.UpdateSizeGuide;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymKitten.API.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin,admin")]
public class AdminSizeGuidesController : ControllerBase
{
    private readonly ISender _sender;

    public AdminSizeGuidesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("api/admin/products/{productId:guid}/size-guide")]
    public async Task<IResult> CreateSizeGuide(
        Guid productId,
        [FromBody] CreateSizeGuideRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateSizeGuideCommand(
            productId,
            request.Size,
            request.ChestCm,
            request.WaistCm,
            request.HipsCm,
            request.HeightRangeCm);

        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPut("api/admin/products/size-guide/{guideId:guid}")]
    public async Task<IResult> UpdateSizeGuide(
        Guid guideId,
        [FromBody] UpdateSizeGuideRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateSizeGuideCommand(
            guideId,
            request.Size,
            request.ChestCm,
            request.WaistCm,
            request.HipsCm,
            request.HeightRangeCm);

        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
