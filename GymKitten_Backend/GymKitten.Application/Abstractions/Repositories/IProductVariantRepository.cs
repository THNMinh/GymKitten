using GymKitten.Domain.Entities;

namespace GymKitten.Application.Abstractions.Repositories;

public interface IProductVariantRepository
{
    Task<Productvariant?> GetByIdAsync(Guid variantId, CancellationToken cancellationToken = default);

    Task<List<Productvariant>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);

    Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken = default);

    Task<bool> ExistsBySkuExcludingIdAsync(string sku, Guid variantId, CancellationToken cancellationToken = default);

    Task AddAsync(Productvariant variant, CancellationToken cancellationToken = default);

    void Update(Productvariant variant);

    void Remove(Productvariant variant);
}
