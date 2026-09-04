using GymKitten.Domain.Entities;

namespace GymKitten.Application.Abstractions.Repositories;

public interface ISizeGuideRepository
{
    Task<List<Sizeguide>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);

    Task<Sizeguide?> GetByIdAsync(Guid guideId, CancellationToken cancellationToken = default);

    Task AddAsync(Sizeguide sizeGuide, CancellationToken cancellationToken = default);

    void Update(Sizeguide sizeGuide);
}
