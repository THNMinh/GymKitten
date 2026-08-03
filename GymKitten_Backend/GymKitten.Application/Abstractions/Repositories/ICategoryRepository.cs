using GymKitten.Domain.Entities;

namespace GymKitten.Application.Abstractions.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid categoryId, CancellationToken cancellationToken = default);

    Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<bool> ExistsBySlugExcludingIdAsync(string slug, Guid categoryId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByIdAsync(Guid categoryId, CancellationToken cancellationToken = default);

    Task<bool> HasProductsAsync(Guid categoryId, CancellationToken cancellationToken = default);

    Task AddAsync(Category category, CancellationToken cancellationToken = default);

    void Update(Category category);

    void Remove(Category category);
}
