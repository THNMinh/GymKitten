using System.Collections.Generic;
using GymKitten.Application.Features.Admin.Users.DTOs;

namespace GymKitten.Application.Features.Admin.Users.Queries.GetAdminUsers;

public sealed record GetAdminUsersResponse(
    IEnumerable<AdminUserItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
