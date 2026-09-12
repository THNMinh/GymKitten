namespace GymKitten.Application.Features.Admin.Inventory.Queries.GetInventory;

public sealed record InventoryItemDto(
    Guid VariantId,
    string Sku,
    string ProductName,
    string Color,
    string? ColorHex,
    string Size,
    int QuantityOnHand,
    int QuantityReserved,
    int AvailableStock);
