using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymKitten.Infrastructure.Repositories;

public sealed class InventoryTransactionRepository : IInventoryTransactionRepository
{
    private readonly GymkittenContext _context;

    public InventoryTransactionRepository(GymkittenContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Inventorytransaction transaction, CancellationToken cancellationToken = default)
    {
        await _context.Inventorytransactions.AddAsync(transaction, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<Inventorytransaction> transactions, CancellationToken cancellationToken = default)
    {
        await _context.Inventorytransactions.AddRangeAsync(transactions, cancellationToken);
    }

    public async Task<(IEnumerable<Inventorytransaction> Items, int TotalCount)> SearchTransactionsAsync(
        Guid? variantId,
        string? sku,
        string? type,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Inventorytransactions
            .AsNoTracking()
            .Include(t => t.Variant)
                .ThenInclude(v => v.Product)
            .Where(t => t.Deletedat == null);

        if (variantId.HasValue)
        {
            query = query.Where(t => t.Variantid == variantId.Value);
        }

        if (!string.IsNullOrWhiteSpace(sku))
        {
            var skuClean = sku.Trim();
            query = query.Where(t => EF.Functions.ILike(t.Variant.Sku, $"%{skuClean}%"));
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            var typeClean = type.Trim();
            query = query.Where(t => EF.Functions.ILike(t.Type, typeClean));
        }

        if (fromDate.HasValue)
        {
            query = query.Where(t => t.Createdat >= fromDate.Value.ToUniversalTime());
        }

        if (toDate.HasValue)
        {
            query = query.Where(t => t.Createdat <= toDate.Value.ToUniversalTime());
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.Createdat)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
