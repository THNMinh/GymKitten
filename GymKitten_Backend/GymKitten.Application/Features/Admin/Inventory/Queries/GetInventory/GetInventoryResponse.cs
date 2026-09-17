namespace GymKitten.Application.Features.Admin.Inventory.Queries.GetInventory;

public sealed record GetInventoryResponse(
    List<InventoryItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    List<ProductInventoryGroupDto>? GroupedProducts = null);
