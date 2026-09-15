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
            .Include(p => p.Productreviews)
            .FirstOrDefaultAsync(p => p.Slug == slug, cancellationToken);
    }

    public async Task<Product?> GetProductWithImagesAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .Include(p => p.Productimages)
            .Include(p => p.Productvariants)
                .ThenInclude(v => v.Inventoryitem)
            .Include(p => p.Productreviews)
            .FirstOrDefaultAsync(p => p.Productid == productId, cancellationToken);
    }

    public async Task<(IEnumerable<Product> Products, int Total)> SearchProductsAsync(
        string? searchName,
        string? gender,
        string? fitType,
        Guid? categoryId,
        string? categorySlug,
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
            .Include(p => p.Category)
            .Include(p => p.Productimages)
            .Include(p => p.Productreviews)
            .Include(p => p.Productvariants)
                .ThenInclude(v => v.Inventoryitem)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchName))
        {
            var term = searchName.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                (p.Description != null && p.Description.ToLower().Contains(term)) ||
                (p.Fittype != null && p.Fittype.ToLower().Contains(term)) ||
                (p.Category != null && p.Category.Name.ToLower().Contains(term)) ||
                p.Productvariants.Any(v => v.Sku.ToLower().Contains(term) || v.Colorname.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(gender) && !gender.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            var g = gender.Trim().ToLower();
            if (g == "men" || g == "nam" || g == "male")
            {
                query = query.Where(p => p.Gender.ToLower() == "men" || p.Gender.ToLower() == "nam" || p.Gender.ToLower() == "male");
            }
            else if (g == "women" || g == "nu" || g == "nữ" || g == "female")
            {
                query = query.Where(p => p.Gender.ToLower() == "women" || p.Gender.ToLower() == "nu" || p.Gender.ToLower() == "nữ" || p.Gender.ToLower() == "female");
            }
            else if (g == "unisex")
            {
                query = query.Where(p => p.Gender.ToLower() == "unisex");
            }
            else
            {
                query = query.Where(p => p.Gender.ToLower() == g);
            }
        }

        if (!string.IsNullOrWhiteSpace(fitType))
        {
            var fitTerm = fitType.Trim().ToLower();
            query = query.Where(p => p.Fittype != null && (p.Fittype.ToLower().Contains(fitTerm) || fitTerm.Contains(p.Fittype.ToLower())));
        }

        if (!string.IsNullOrWhiteSpace(categorySlug))
        {
            var slugTerm = categorySlug.Trim().ToLower();
            query = query.Where(p => p.Category != null && (p.Category.Slug.ToLower() == slugTerm || p.Category.Name.ToLower().Contains(slugTerm)));
        }

        if (categoryId.HasValue && categoryId.Value != Guid.Empty)
        {
            query = query.Where(p => p.Categoryid == categoryId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(p => p.Isactive == isActive.Value);
        }

        // Flexible Fuzzy Filtering on Productvariants Colorname
        if (colors != null && colors.Count > 0)
        {
            var colorTerms = colors
                .Select(c => c.Trim().ToLower())
                .Where(c => !string.IsNullOrEmpty(c))
                .ToList();

            if (colorTerms.Count > 0)
            {
                query = query.Where(p => p.Productvariants.Any(v =>
                    colorTerms.Any(c =>
                        v.Colorname.ToLower().Contains(c) ||
                        c.Contains(v.Colorname.ToLower()) ||
                        (c == "black" && (v.Colorname.ToLower().Contains("onyx") || v.Colorname.ToLower().Contains("đen"))) ||
                        (c == "blue" && (v.Colorname.ToLower().Contains("navy") || v.Colorname.ToLower().Contains("cyan") || v.Colorname.ToLower().Contains("sky"))) ||
                        (c == "pink" && v.Colorname.ToLower().Contains("rose")) ||
                        (c == "grey" && v.Colorname.ToLower().Contains("gray")) ||
                        (c == "gray" && v.Colorname.ToLower().Contains("grey")) ||
                        (c == "purple" && v.Colorname.ToLower().Contains("violet")) ||
                        (c == "violet" && v.Colorname.ToLower().Contains("purple"))
                    )
                ));
            }
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
