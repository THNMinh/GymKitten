using GymKitten.Domain.Entities;

namespace GymKitten.Application.Abstractions.Repositories;

public interface IPaymentTransactionRepository
{
    Task<Paymenttransaction?> GetByTransactionIdAsync(Guid transactionId, CancellationToken cancellationToken = default);

    Task AddAsync(Paymenttransaction transaction, CancellationToken cancellationToken = default);

    void Update(Paymenttransaction transaction);
}
