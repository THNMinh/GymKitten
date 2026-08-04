using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymKitten.Infrastructure.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly GymkittenContext _context;

    public CategoryRepository(GymkittenContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Categoryid == categoryId, cancellationToken);
    }

    public async Task<(IEnumerable<Category> Categories, int Total)> SearchCategoriesAsync(
        string? searchName,
        Guid? parentCategoryId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Categories
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchName))
        {
            var term = searchName.Trim().ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(term));
        }

        if (parentCategoryId.HasValue && parentCategoryId.Value != Guid.Empty)
        {
            query = query.Where(c => c.Parentcategoryid == parentCategoryId.Value);
        }

        // Count First
        var total = await query.CountAsync(cancellationToken);

        // Take Later
        var categories = await query
            .OrderBy(c => c.Displayorder)
            .ThenBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (categories, total);
    }

    public async Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AnyAsync(c => c.Slug == slug, cancellationToken);
    }

    public async Task<bool> ExistsBySlugExcludingIdAsync(string slug, Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AnyAsync(c => c.Slug == slug && c.Categoryid != categoryId, cancellationToken);
    }

    public async Task<bool> ExistsByIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AnyAsync(c => c.Categoryid == categoryId, cancellationToken);
    }

    public async Task<bool> HasProductsAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AnyAsync(p => p.Categoryid == categoryId, cancellationToken);
    }

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        await _context.Categories.AddAsync(category, cancellationToken);
    }

    public void Update(Category category)
    {
        _context.Categories.Update(category);
    }

    public void Remove(Category category)
    {
        _context.Categories.Remove(category);
    }
}
