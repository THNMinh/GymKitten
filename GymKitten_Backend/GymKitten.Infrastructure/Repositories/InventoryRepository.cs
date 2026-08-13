using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymKitten.Infrastructure.Repositories;

public sealed class InventoryRepository : IInventoryRepository
{
    private readonly GymkittenContext _context;

    public InventoryRepository(GymkittenContext context)
    {
        _context = context;
    }

    public async Task<Inventoryitem?> GetByVariantIdAsync(Guid variantId, CancellationToken cancellationToken = default)
    {
        return await _context.Inventoryitems
            .FirstOrDefaultAsync(i => i.Variantid == variantId, cancellationToken);
    }

    public async Task<List<Inventoryitem>> GetByVariantIdsAsync(IEnumerable<Guid> variantIds, CancellationToken cancellationToken = default)
    {
        var ids = variantIds.Distinct().ToList();
        return await _context.Inventoryitems
            .Where(i => ids.Contains(i.Variantid))
            .ToListAsync(cancellationToken);
    }

    public void Update(Inventoryitem inventoryItem)
    {
        _context.Inventoryitems.Update(inventoryItem);
    }

    public void UpdateRange(IEnumerable<Inventoryitem> inventoryItems)
    {
        _context.Inventoryitems.UpdateRange(inventoryItems);
    }
}
