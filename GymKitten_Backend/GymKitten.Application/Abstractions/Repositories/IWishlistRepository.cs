using GymKitten.Domain.Entities;

namespace GymKitten.Application.Abstractions.Repositories;

public interface IWishlistRepository
{
    Task<bool> ExistsAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);

    Task<bool> ToggleWishlistAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);

    Task<int> ExecuteHardDeleteAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);

    Task<(IEnumerable<Wishlist> Wishlists, int Total)> GetWishlistByUserIdAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
