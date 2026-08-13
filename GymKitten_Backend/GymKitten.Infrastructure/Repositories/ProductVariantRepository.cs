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
        return await _context.Productvariants
            .AnyAsync(v => v.Sku == sku, cancellationToken);
    }

    public async Task<bool> ExistsBySkuExcludingIdAsync(string sku, Guid variantId, CancellationToken cancellationToken = default)
    {
        return await _context.Productvariants
            .AnyAsync(v => v.Sku == sku && v.Variantid != variantId, cancellationToken);
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
