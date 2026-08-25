namespace GymKitten.Application.Features.Admin.Orders.Commands.ShipOrder;

public sealed record ShipOrderCommandResponse(
    Guid OrderId,
    string Status);
