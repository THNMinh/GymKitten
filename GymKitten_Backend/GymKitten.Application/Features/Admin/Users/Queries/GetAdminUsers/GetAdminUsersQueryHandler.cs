using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Features.Admin.Users.DTOs;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Admin.Users.Queries.GetAdminUsers;

public sealed class GetAdminUsersQueryHandler
    : IQueryHandler<GetAdminUsersQuery, Result<GetAdminUsersResponse>>
{
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;

    public GetAdminUsersQueryHandler(
        IUserContext userContext,
        IUserRepository userRepository)
    {
        _userContext = userContext;
        _userRepository = userRepository;
    }

    public async Task<Result<GetAdminUsersResponse>> Handle(
        GetAdminUsersQuery request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(_userContext.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<GetAdminUsersResponse>(UserErrors.Forbidden);
        }

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        var (users, totalCount) = await _userRepository.SearchUsersAsync(
            request.SearchTerm,
            request.Role,
            request.IsActive,
            page,
            pageSize,
            cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var dtos = users.Select(u => new AdminUserItemDto(
            u.Userid,
            u.Email,
            u.Fullname,
            u.Phone,
            u.Role,
            u.Isemailverified,
            u.Isactive,
            u.Avatarurl,
            u.Createdat,
            u.Updatedat));

        return Result.Success(new GetAdminUsersResponse(
            dtos,
            totalCount,
            page,
            pageSize,
            totalPages));
    }
}
