using System;
using System.Threading;
using System.Threading.Tasks;
using GymKitten.API.Extensions;
using GymKitten.API.Requests;
using GymKitten.Application.Features.Admin.Users.Commands.CreateUser;
using GymKitten.Application.Features.Admin.Users.Commands.DeleteUser;
using GymKitten.Application.Features.Admin.Users.Commands.UpdateUser;
using GymKitten.Application.Features.Admin.Users.Queries.GetAdminUserById;
using GymKitten.Application.Features.Admin.Users.Queries.GetAdminUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GymKitten.API.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin,admin")]
[Route("api/admin/users")]
public class AdminUsersController : ControllerBase
{
    private readonly ISender _sender;

    public AdminUsersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IResult> GetUsers(
        [FromQuery] string? searchTerm,
        [FromQuery] string? role,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAdminUsersQuery(searchTerm, role, isActive, page, pageSize);
        var result = await _sender.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("{id:guid}")]
    public async Task<IResult> GetUserById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAdminUserByIdQuery(id);
        var result = await _sender.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost]
    public async Task<IResult> CreateUser(
        [FromBody] CreateAdminUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateUserCommand(
            request.Email,
            request.Password,
            request.FullName,
            request.Phone,
            request.Role,
            request.IsActive);

        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPut("{id:guid}")]
    public async Task<IResult> UpdateUser(
        [FromRoute] Guid id,
        [FromBody] UpdateAdminUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateUserCommand(
            id,
            request.FullName,
            request.Phone,
            request.Role,
            request.IsActive,
            request.IsEmailVerified);

        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IResult> DeleteUser(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteUserCommand(id);
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
