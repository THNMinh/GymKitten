using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymKitten.Infrastructure.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly GymkittenContext _context;

    public ProductRepository(GymkittenContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Productid == productId, cancellationToken);
    }

    public async Task<Product?> GetProductWithImagesAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Productimages)
            .FirstOrDefaultAsync(p => p.Productid == productId, cancellationToken);
    }

    public async Task<List<Product>> GetAllPagedAsync(
        int page,
        int pageSize,
        string? searchTerm,
        Guid? categoryId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Products.AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.Categoryid == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term) || p.Slug.ToLower().Contains(term));
        }

        return await query
            .OrderByDescending(p => p.Createdat)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(
        string? searchTerm,
        Guid? categoryId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Products.AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.Categoryid == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term) || p.Slug.ToLower().Contains(term));
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AnyAsync(p => p.Slug == slug, cancellationToken);
    }

    public async Task<bool> ExistsBySlugExcludingIdAsync(string slug, Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AnyAsync(p => p.Slug == slug && p.Productid != productId, cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(product, cancellationToken);
    }

    public void Update(Product product)
    {
        _context.Products.Update(product);
    }

    public void Remove(Product product)
    {
        _context.Products.Remove(product);
    }
}
