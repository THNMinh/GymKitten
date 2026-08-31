using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Admin.Orders.Commands.UpdateOrderStatus;

public sealed record UpdateOrderStatusCommand(
    Guid OrderId,
    string Status,
    string? Title = null,
    string? Description = null,
    string? Location = null) : ICommand<Result<UpdateOrderStatusResponse>>;

public sealed record UpdateOrderStatusResponse(
    Guid OrderId,
    string Status,
    string Message);
