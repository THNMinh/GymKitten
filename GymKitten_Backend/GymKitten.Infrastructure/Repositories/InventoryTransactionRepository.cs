using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Entities;

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
}
