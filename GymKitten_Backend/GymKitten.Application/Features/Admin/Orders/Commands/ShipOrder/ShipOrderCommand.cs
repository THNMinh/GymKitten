using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Admin.Orders.Commands.ShipOrder;

public sealed record ShipOrderCommand(
    Guid OrderId) : ICommand<Result<ShipOrderCommandResponse>>;
