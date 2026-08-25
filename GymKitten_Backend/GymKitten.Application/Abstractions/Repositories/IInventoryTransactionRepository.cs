using GymKitten.Domain.Entities;

namespace GymKitten.Application.Abstractions.Repositories;

public interface IInventoryTransactionRepository
{
    Task AddAsync(Inventorytransaction transaction, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<Inventorytransaction> transactions, CancellationToken cancellationToken = default);
}
