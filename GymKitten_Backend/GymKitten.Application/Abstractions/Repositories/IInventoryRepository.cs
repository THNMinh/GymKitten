using GymKitten.Application.Features.Admin.Inventory.Queries.GetInventory;
using GymKitten.Domain.Entities;

namespace GymKitten.Application.Abstractions.Repositories;

public interface IInventoryRepository
{
    Task<Inventoryitem?> GetByVariantIdAsync(Guid variantId, CancellationToken cancellationToken = default);

    Task<List<Inventoryitem>> GetByVariantIdsAsync(IEnumerable<Guid> variantIds, CancellationToken cancellationToken = default);

    Task<(IEnumerable<InventoryItemDto> Items, int TotalCount)> SearchInventoryAsync(
        string? sku,
        string? productName,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task AddAsync(Inventoryitem inventoryItem, CancellationToken cancellationToken = default);

    void Update(Inventoryitem inventoryItem);

    void UpdateRange(IEnumerable<Inventoryitem> inventoryItems);
}
