using GymKitten.Domain.Entities;

namespace GymKitten.Application.Abstractions.Repositories;

public interface ICartRepository
{
    Task<Cart?> GetActiveCartByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Cart?> GetCartWithDetailsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Cartitem?> GetCartItemAsync(Guid cartId, Guid variantId, CancellationToken cancellationToken = default);

    Task AddCartAsync(Cart cart, CancellationToken cancellationToken = default);

    Task AddCartItemAsync(Cartitem cartItem, CancellationToken cancellationToken = default);

    void UpdateCart(Cart cart);

    void UpdateCartItem(Cartitem cartItem);

    void RemoveCartItem(Cartitem cartItem);

    Task ClearCartItemsAsync(Guid cartId, CancellationToken cancellationToken = default);
}
