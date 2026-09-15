using System;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Admin.Users.Commands.DeleteUser;

public sealed record DeleteUserCommand(Guid UserId) : ICommand<Result>;
