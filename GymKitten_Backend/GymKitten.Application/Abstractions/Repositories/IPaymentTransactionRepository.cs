using GymKitten.Application.Features.Admin.Finance.Queries.GetTransactions;
using GymKitten.Domain.Entities;

namespace GymKitten.Application.Abstractions.Repositories;

public interface IPaymentTransactionRepository
{
    Task<Paymenttransaction?> GetByTransactionIdAsync(Guid transactionId, CancellationToken cancellationToken = default);

    Task<Paymenttransaction?> GetByTxnRefAsync(string txnRef, CancellationToken cancellationToken = default);

    Task<Paymenttransaction?> GetByOrderIdAndGatewayAsync(Guid orderId, string gateway, CancellationToken cancellationToken = default);

    Task<(List<TransactionDto> Items, int TotalCount)> GetTransactionsPagedAsync(
        DateTime? startDate,
        DateTime? endDate,
        string? status,
        string? gateway,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task AddAsync(Paymenttransaction transaction, CancellationToken cancellationToken = default);

    void Update(Paymenttransaction transaction);
}
