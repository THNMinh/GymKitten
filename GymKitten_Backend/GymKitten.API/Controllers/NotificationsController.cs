using GymKitten.API.Extensions;
using GymKitten.Application.Features.Notifications.Commands.MarkAllAsRead;
using GymKitten.Application.Features.Notifications.Commands.MarkAsRead;
using GymKitten.Application.Features.Notifications.Queries.GetMyNotifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GymKitten.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly ISender _sender;

    public NotificationsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// GET /api/notifications — Lấy danh sách thông báo của tôi (Kèm unreadCount)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> GetMyNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMyNotificationsQuery(page, pageSize);
        var result = await _sender.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// PUT /api/notifications/{id}/read — Đánh dấu 1 thông báo là đã đọc
    /// </summary>
    [HttpPut("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> MarkAsRead(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new MarkNotificationAsReadCommand(id);
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// PUT /api/notifications/read-all — Đánh dấu tất cả thông báo của tôi là đã đọc
    /// </summary>
    [HttpPut("read-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> MarkAllAsRead(CancellationToken cancellationToken = default)
    {
        var command = new MarkAllNotificationsAsReadCommand();
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
