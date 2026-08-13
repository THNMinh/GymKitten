using GymKitten.Domain.Entities;

namespace GymKitten.Application.Abstractions.Repositories;

public interface IInventoryRepository
{
    Task<Inventoryitem?> GetByVariantIdAsync(Guid variantId, CancellationToken cancellationToken = default);

    Task<List<Inventoryitem>> GetByVariantIdsAsync(IEnumerable<Guid> variantIds, CancellationToken cancellationToken = default);

    void Update(Inventoryitem inventoryItem);

    void UpdateRange(IEnumerable<Inventoryitem> inventoryItems);
}
