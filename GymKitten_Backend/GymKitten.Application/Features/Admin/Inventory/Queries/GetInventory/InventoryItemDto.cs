namespace GymKitten.Application.Features.Admin.Inventory.Queries.GetInventory;

public sealed record InventoryItemDto(
    Guid VariantId,
    string Sku,
    Guid ProductId,
    string ProductName,
    string Color,
    string? ColorHex,
    string Size,
    int QuantityOnHand,
    int QuantityReserved,
    int AvailableStock);

public sealed record ProductInventoryGroupDto(
    Guid ProductId,
    string ProductName,
    int QuantityOnHand,
    int QuantityReserved,
    int AvailableStock,
    int TotalVariants,
    List<ColorInventoryGroupDto> Colors);

public sealed record ColorInventoryGroupDto(
    string Color,
    string? ColorHex,
    int QuantityOnHand,
    int QuantityReserved,
    int AvailableStock,
    List<InventoryItemDto> Sizes,
    string? ColorName = null);
