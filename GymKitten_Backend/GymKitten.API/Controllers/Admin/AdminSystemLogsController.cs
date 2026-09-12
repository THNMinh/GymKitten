using GymKitten.API.Extensions;
using GymKitten.Application.Features.Admin.SystemLogs.Queries.GetSystemLogById;
using GymKitten.Application.Features.Admin.SystemLogs.Queries.GetSystemLogs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymKitten.API.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin,admin")]
[Route("api/admin/system-logs")]
public class AdminSystemLogsController : ControllerBase
{
    private readonly ISender _sender;

    public AdminSystemLogsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IResult> GetSystemLogs(
        [FromQuery] GetSystemLogsQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("{logId:guid}")]
    public async Task<IResult> GetSystemLogById(
        [FromRoute] Guid logId,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetSystemLogByIdQuery(logId), cancellationToken);
        return result.MatchOk();
    }
}
