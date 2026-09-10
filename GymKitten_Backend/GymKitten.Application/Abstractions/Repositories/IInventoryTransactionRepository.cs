using GymKitten.Domain.Entities;

namespace GymKitten.Application.Abstractions.Repositories;

public interface IInventoryTransactionRepository
{
    Task AddAsync(Inventorytransaction transaction, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<Inventorytransaction> transactions, CancellationToken cancellationToken = default);

    Task<(IEnumerable<Inventorytransaction> Items, int TotalCount)> SearchTransactionsAsync(
        Guid? variantId,
        string? sku,
        string? type,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
