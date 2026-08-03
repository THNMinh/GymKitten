using GymKitten.Domain.Entities;

namespace GymKitten.Application.Abstractions.Repositories;

public interface IProductImageRepository
{
    Task<bool> ProductExistsAsync(Guid productId, CancellationToken cancellationToken = default);

    Task<Productimage?> GetByIdAsync(Guid imageId, CancellationToken cancellationToken = default);

    Task<List<Productimage>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);

    Task<int> GetMaxDisplayOrderAsync(Guid productId, CancellationToken cancellationToken = default);

    Task AddPhotosAsync(List<Productimage> photos, CancellationToken cancellationToken = default);

    void Remove(Productimage photo);
}
