using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymKitten.Infrastructure.Repositories;

public sealed class CartRepository : ICartRepository
{
    private readonly GymkittenContext _context;

    public CartRepository(GymkittenContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetActiveCartByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Carts
            .FirstOrDefaultAsync(c => c.Userid == userId && c.Deletedat == null, cancellationToken);
    }

    public async Task<Cart?> GetCartWithDetailsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Carts
            .AsNoTracking()
            .Where(c => c.Userid == userId && c.Deletedat == null)
            .Include(c => c.Cartitems.Where(ci => ci.Deletedat == null))
                .ThenInclude(ci => ci.Variant)
                    .ThenInclude(v => v.Product)
                        .ThenInclude(p => p.Productimages)
            .Include(c => c.Cartitems.Where(ci => ci.Deletedat == null))
                .ThenInclude(ci => ci.Variant)
                    .ThenInclude(v => v.Inventoryitem)
            .Include(c => c.Cartitems.Where(ci => ci.Deletedat == null))
                .ThenInclude(ci => ci.Variant)
                    .ThenInclude(v => v.Productimages)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Cartitem?> GetCartItemAsync(Guid cartId, Guid variantId, CancellationToken cancellationToken = default)
    {
        return await _context.Cartitems
            .FirstOrDefaultAsync(ci => ci.Cartid == cartId && ci.Variantid == variantId && ci.Deletedat == null, cancellationToken);
    }

    public async Task AddCartAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        await _context.Carts.AddAsync(cart, cancellationToken);
    }

    public async Task AddCartItemAsync(Cartitem cartItem, CancellationToken cancellationToken = default)
    {
        await _context.Cartitems.AddAsync(cartItem, cancellationToken);
    }

    public void UpdateCart(Cart cart)
    {
        _context.Carts.Update(cart);
    }

    public void UpdateCartItem(Cartitem cartItem)
    {
        _context.Cartitems.Update(cartItem);
    }

    public void RemoveCartItem(Cartitem cartItem)
    {
        _context.Cartitems.Remove(cartItem);
    }

    public async Task ClearCartItemsAsync(Guid cartId, CancellationToken cancellationToken = default)
    {
        await _context.Cartitems
            .Where(ci => ci.Cartid == cartId && ci.Deletedat == null)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
