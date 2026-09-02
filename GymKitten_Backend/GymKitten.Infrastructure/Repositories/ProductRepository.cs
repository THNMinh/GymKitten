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

    public async Task<Productvariant?> GetVariantByIdAsync(Guid variantId, CancellationToken cancellationToken = default)
    {
        return await _context.Productvariants
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Variantid == variantId, cancellationToken);
    }

    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .Include(p => p.Productimages)
            .Include(p => p.Productvariants)
                .ThenInclude(v => v.Inventoryitem)
            .FirstOrDefaultAsync(p => p.Slug == slug, cancellationToken);
    }

    public async Task<Product?> GetProductWithImagesAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .Include(p => p.Productimages)
            .Include(p => p.Productvariants)
                .ThenInclude(v => v.Inventoryitem)
            .FirstOrDefaultAsync(p => p.Productid == productId, cancellationToken);
    }

    public async Task<(IEnumerable<Product> Products, int Total)> SearchProductsAsync(
        string? searchName,
        string? gender,
        string? fitType,
        Guid? categoryId,
        bool? isActive,
        List<string>? colors,
        List<string>? sizes,
        decimal? minPrice,
        decimal? maxPrice,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Productimages)
            .Include(p => p.Productvariants)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchName))
        {
            var term = searchName.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(gender))
        {
            var genderTerm = gender.Trim().ToLower();
            query = query.Where(p => p.Gender.ToLower() == genderTerm);
        }

        if (!string.IsNullOrWhiteSpace(fitType))
        {
            var fitTerm = fitType.Trim().ToLower();
            query = query.Where(p => p.Fittype != null && p.Fittype.ToLower() == fitTerm);
        }

        if (categoryId.HasValue && categoryId.Value != Guid.Empty)
        {
            query = query.Where(p => p.Categoryid == categoryId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(p => p.Isactive == isActive.Value);
        }

        // Deep Filtering on Productvariants
        if (colors != null && colors.Count > 0)
        {
            var colorSet = colors.Select(c => c.Trim().ToLower()).ToList();
            query = query.Where(p => p.Productvariants.Any(v => colorSet.Contains(v.Colorname.ToLower())));
        }

        if (sizes != null && sizes.Count > 0)
        {
            var sizeSet = sizes.Select(s => s.Trim().ToLower()).ToList();
            query = query.Where(p => p.Productvariants.Any(v => sizeSet.Contains(v.Size.ToLower())));
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Productvariants.Any(v => v.Price >= minPrice.Value));
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Productvariants.Any(v => v.Price <= maxPrice.Value));
        }

        // Count First
        var total = await query.CountAsync(cancellationToken);

        // Take Later
        var products = await query
            .OrderByDescending(p => p.Createdat)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (products, total);
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
