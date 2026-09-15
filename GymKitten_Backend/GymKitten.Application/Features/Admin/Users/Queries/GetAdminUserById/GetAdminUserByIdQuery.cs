using System;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Features.Admin.Users.DTOs;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Admin.Users.Queries.GetAdminUserById;

public sealed record GetAdminUserByIdQuery(Guid UserId) : IQuery<Result<AdminUserDetailsResponse>>;
