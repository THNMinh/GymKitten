using GymKitten.Domain.Entities;

namespace GymKitten.Application.Abstractions.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default);

    Task<Product?> GetProductWithImagesAsync(Guid productId, CancellationToken cancellationToken = default);

    Task<(IEnumerable<Product> Products, int Total)> SearchProductsAsync(
        string? searchName,
        string? gender,
        string? fitType,
        Guid? categoryId,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<bool> ExistsBySlugExcludingIdAsync(string slug, Guid productId, CancellationToken cancellationToken = default);

    Task AddAsync(Product product, CancellationToken cancellationToken = default);

    void Update(Product product);

    void Remove(Product product);
}
