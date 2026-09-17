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
        var productQuery = _context.Products
            .AsNoTracking()
            .Where(p => p.Deletedat == null && p.Productvariants.Any(v => v.Deletedat == null))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(productName))
        {
            var nameTerm = productName.Trim().ToLower();
            productQuery = productQuery.Where(p => p.Name.ToLower().Contains(nameTerm));
        }

        if (!string.IsNullOrWhiteSpace(sku))
        {
            var skuTerm = sku.Trim().ToLower();
            productQuery = productQuery.Where(p => p.Productvariants.Any(v => v.Deletedat == null && v.Sku.ToLower().Contains(skuTerm)));
        }

        var totalCount = await productQuery.CountAsync(cancellationToken);

        var pagedProducts = await productQuery
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(p => p.Productvariants.Where(v => v.Deletedat == null))
                .ThenInclude(v => v.Inventoryitem)
            .ToListAsync(cancellationToken);

        var items = new List<InventoryItemDto>();

        foreach (var p in pagedProducts)
        {
            var variants = p.Productvariants
                .Where(v => v.Deletedat == null)
                .OrderBy(v => v.Colorname)
                .ThenBy(v => v.Size);

            foreach (var v in variants)
            {
                if (!string.IsNullOrWhiteSpace(sku))
                {
                    var skuTerm = sku.Trim().ToLower();
                    if (!v.Sku.ToLower().Contains(skuTerm))
                        continue;
                }

                var onHand = v.Inventoryitem?.Quantityonhand ?? 0;
                var reserved = v.Inventoryitem?.Quantityreserved ?? 0;
                var available = Math.Max(0, onHand - reserved);

                items.Add(new InventoryItemDto(
                    v.Variantid,
                    v.Sku,
                    p.Productid,
                    p.Name,
                    v.Colorname,
                    v.Colorhex,
                    v.Size,
                    onHand,
                    reserved,
                    available));
            }
        }

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
