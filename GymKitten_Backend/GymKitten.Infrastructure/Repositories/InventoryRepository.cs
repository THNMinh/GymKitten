using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Features.Admin.Inventory.Queries.GetInventory;
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

    public async Task<(IEnumerable<InventoryItemDto> Items, int TotalCount)> SearchInventoryAsync(
        string? sku,
        string? productName,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Inventoryitems
            .AsNoTracking()
            .Include(i => i.Variant)
                .ThenInclude(v => v.Product)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(sku))
        {
            var skuTerm = sku.Trim().ToLower();
            query = query.Where(i => i.Variant.Sku.ToLower().Contains(skuTerm));
        }

        if (!string.IsNullOrWhiteSpace(productName))
        {
            var nameTerm = productName.Trim().ToLower();
            query = query.Where(i => i.Variant.Product.Name.ToLower().Contains(nameTerm));
        }

        // Count First
        var totalCount = await query.CountAsync(cancellationToken);

        // Take Later
        var items = await query
            .OrderBy(i => i.Variant.Sku)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new InventoryItemDto(
                i.Variantid,
                i.Variant.Sku,
                i.Variant.Product.Name,
                i.Variant.Colorname,
                i.Variant.Size,
                i.Quantityonhand,
                i.Quantityreserved,
                i.Quantityonhand - i.Quantityreserved))
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Inventoryitem inventoryItem, CancellationToken cancellationToken = default)
    {
        await _context.Inventoryitems.AddAsync(inventoryItem, cancellationToken);
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
