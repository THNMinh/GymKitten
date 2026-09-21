using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymKitten.Infrastructure.Repositories;

public sealed class ProductImageRepository : IProductImageRepository
{
    private readonly GymkittenContext _context;

    public ProductImageRepository(GymkittenContext context)
    {
        _context = context;
    }

    public async Task<bool> ProductExistsAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AnyAsync(p => p.Productid == productId, cancellationToken);
    }

    public async Task<Productimage?> GetByIdAsync(Guid imageId, CancellationToken cancellationToken = default)
    {
        return await _context.Productimages
            .FirstOrDefaultAsync(i => i.Imageid == imageId, cancellationToken);
    }

    public async Task<List<Productimage>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.Productimages
            .Include(i => i.Variant)
            .Where(i => i.Productid == productId)
            .OrderBy(i => i.Displayorder)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetMaxDisplayOrderAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var orders = await _context.Productimages
            .Where(i => i.Productid == productId)
            .Select(i => i.Displayorder)
            .ToListAsync(cancellationToken);

        return orders.Count == 0 ? 0 : orders.Max();
    }

    public async Task AddPhotosAsync(List<Productimage> photos, CancellationToken cancellationToken = default)
    {
        await _context.Productimages.AddRangeAsync(photos, cancellationToken);
    }

    public void Remove(Productimage photo)
    {
        _context.Productimages.Remove(photo);
    }
}
