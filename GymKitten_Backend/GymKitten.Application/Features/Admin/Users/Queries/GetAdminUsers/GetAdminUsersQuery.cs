using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Admin.Users.Queries.GetAdminUsers;

public sealed record GetAdminUsersQuery(
    string? SearchTerm = null,
    string? Role = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 10) : IQuery<Result<GetAdminUsersResponse>>;
