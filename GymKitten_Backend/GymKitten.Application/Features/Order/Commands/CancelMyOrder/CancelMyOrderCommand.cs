using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Order.Commands.CancelMyOrder;

public sealed record CancelMyOrderCommand(Guid OrderId) : ICommand<Result<CancelMyOrderResponse>>;

public sealed record CancelMyOrderResponse(
    Guid OrderId,
    string Status,
    string Message);
