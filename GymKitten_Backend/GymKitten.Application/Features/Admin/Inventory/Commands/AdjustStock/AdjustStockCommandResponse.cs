namespace GymKitten.Application.Features.Admin.Inventory.Commands.AdjustStock;

public sealed record AdjustStockCommandResponse(
    Guid VariantId,
    int NewQuantityOnHand,
    int QuantityReserved);
