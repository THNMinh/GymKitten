using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymKitten.Infrastructure.Repositories;

public sealed class WishlistRepository : IWishlistRepository
{
    private readonly GymkittenContext _context;

    public WishlistRepository(GymkittenContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.Wishlists
            .AnyAsync(w => w.Userid == userId && w.Productid == productId, cancellationToken);
    }

    public async Task<bool> ToggleWishlistAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        var exists = await ExistsAsync(userId, productId, cancellationToken);

        if (exists)
        {
            // ExecuteDirect hard delete bypassing SaveChangesAsync and soft delete interceptor
            await _context.Wishlists
                .Where(w => w.Userid == userId && w.Productid == productId)
                .ExecuteDeleteAsync(cancellationToken);

            return false; // Removed
        }

        var entity = new Wishlist
        {
            Wishlistid = Guid.NewGuid(),
            Userid = userId,
            Productid = productId,
            Createdat = DateTime.UtcNow,
            Updatedat = DateTime.UtcNow
        };

        await _context.Wishlists.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return true; // Added
    }

    public async Task<int> ExecuteHardDeleteAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.Wishlists
            .Where(w => w.Userid == userId && w.Productid == productId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Wishlist> Wishlists, int Total)> SearchWishlistByUserIdAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Wishlists
            .AsNoTracking()
            .Where(w => w.Userid == userId)
            .Include(w => w.Product)
                .ThenInclude(p => p.Productimages)
            .Include(w => w.Product)
                .ThenInclude(p => p.Productvariants);

        // Count First
        var total = await query.CountAsync(cancellationToken);

        // Take Later
        var items = await query
            .OrderByDescending(w => w.Createdat)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}
