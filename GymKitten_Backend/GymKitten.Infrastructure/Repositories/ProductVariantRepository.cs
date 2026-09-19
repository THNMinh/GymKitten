using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymKitten.Infrastructure.Repositories;

public sealed class ProductVariantRepository : IProductVariantRepository
{
    private readonly GymkittenContext _context;

    public ProductVariantRepository(GymkittenContext context)
    {
        _context = context;
    }

    public async Task<Productvariant?> GetByIdAsync(Guid variantId, CancellationToken cancellationToken = default)
    {
        return await _context.Productvariants
            .Include(v => v.Product)
            .Include(v => v.Inventoryitem)
            .FirstOrDefaultAsync(v => v.Variantid == variantId, cancellationToken);
    }

    public async Task<List<Productvariant>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.Productvariants
            .AsNoTracking()
            .Include(v => v.Inventoryitem)
            .Where(v => v.Productid == productId)
            .OrderBy(v => v.Colorname)
            .ThenBy(v => v.Size)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        var normSku = (sku ?? "").Trim().ToLower();
        return await _context.Productvariants
            .IgnoreQueryFilters()
            .AnyAsync(v => v.Sku.ToLower() == normSku, cancellationToken);
    }

    public async Task<bool> ExistsBySkuExcludingIdAsync(string sku, Guid variantId, CancellationToken cancellationToken = default)
    {
        var normSku = (sku ?? "").Trim().ToLower();
        return await _context.Productvariants
            .IgnoreQueryFilters()
            .AnyAsync(v => v.Variantid != variantId && v.Sku.ToLower() == normSku, cancellationToken);
    }

    public async Task<bool> ExistsByProductColorAndSizeAsync(
        Guid productId,
        string colorName,
        string size,
        Guid? excludeVariantId = null,
        CancellationToken cancellationToken = default)
    {
        var targetColor = (colorName ?? "").Trim().ToLower();
        var targetSize = (size ?? "").Trim().ToLower();

        var query = _context.Productvariants
            .Where(v => v.Productid == productId &&
                        v.Colorname.ToLower() == targetColor &&
                        v.Size.ToLower() == targetSize);

        if (excludeVariantId.HasValue)
        {
            query = query.Where(v => v.Variantid != excludeVariantId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> HasOrdersAsync(Guid variantId, CancellationToken cancellationToken = default)
    {
        return await _context.Orderitems
            .AnyAsync(oi => oi.Variantid == variantId, cancellationToken);
    }

    public async Task AddAsync(Productvariant variant, CancellationToken cancellationToken = default)
    {
        await _context.Productvariants.AddAsync(variant, cancellationToken);
    }

    public void Update(Productvariant variant)
    {
        _context.Productvariants.Update(variant);
    }

    public void Remove(Productvariant variant)
    {
        _context.Productvariants.Remove(variant);
    }
}
