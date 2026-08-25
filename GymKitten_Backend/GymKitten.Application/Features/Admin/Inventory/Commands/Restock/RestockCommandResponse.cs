namespace GymKitten.Application.Features.Admin.Inventory.Commands.Restock;

public sealed record RestockCommandResponse(
    Guid VariantId,
    int NewQuantityOnHand,
    int QuantityReserved);
