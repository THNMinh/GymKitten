using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Features.Admin.Finance.Queries.GetTransactions;
using GymKitten.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymKitten.Infrastructure.Repositories;

public sealed class PaymentTransactionRepository : IPaymentTransactionRepository
{
    private readonly GymkittenContext _context;

    public PaymentTransactionRepository(GymkittenContext context)
    {
        _context = context;
    }

    public async Task<Paymenttransaction?> GetByTransactionIdAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        return await _context.Paymenttransactions
            .FirstOrDefaultAsync(t => t.Transactionid == transactionId, cancellationToken);
    }

    public async Task<Paymenttransaction?> GetByTxnRefAsync(string txnRef, CancellationToken cancellationToken = default)
    {
        if (Guid.TryParse(txnRef, out var guidId))
        {
            var matchByGuid = await _context.Paymenttransactions
                .FirstOrDefaultAsync(t => t.Transactionid == guidId, cancellationToken);

            if (matchByGuid != null)
            {
                return matchByGuid;
            }
        }

        return await _context.Paymenttransactions
            .FirstOrDefaultAsync(t => t.Gatewaytransactionid == txnRef, cancellationToken);
    }

    public async Task<Paymenttransaction?> GetByOrderIdAndGatewayAsync(Guid orderId, string gateway, CancellationToken cancellationToken = default)
    {
        var gatewayTerm = gateway.Trim().ToLower();
        return await _context.Paymenttransactions
            .FirstOrDefaultAsync(t => t.Orderid == orderId && t.Gateway.ToLower() == gatewayTerm, cancellationToken);
    }

    public async Task<(IEnumerable<TransactionDto> Items, int TotalCount)> SearchTransactionsAsync(
        DateTime? startDate,
        DateTime? endDate,
        string? status,
        string? gateway,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Paymenttransactions
            .AsNoTracking()
            .Include(t => t.Order)
                .ThenInclude(o => o.User)
            .AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(t => t.Createdat >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.Createdat <= endDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var statusTerm = status.Trim().ToLower();
            query = query.Where(t => t.Status.ToLower() == statusTerm);
        }

        if (!string.IsNullOrWhiteSpace(gateway))
        {
            var gatewayTerm = gateway.Trim().ToLower();
            query = query.Where(t => t.Gateway.ToLower() == gatewayTerm);
        }

        // Count First
        var totalCount = await query.CountAsync(cancellationToken);

        // Take Later
        var items = await query
            .OrderByDescending(t => t.Createdat)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TransactionDto(
                t.Transactionid,
                t.Order != null ? t.Order.Ordercode : string.Empty,
                t.Order != null && t.Order.User != null ? t.Order.User.Email : "Guest",
                t.Gateway,
                t.Amount,
                t.Status,
                t.Createdat,
                t.Paymentdate))
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Paymenttransaction transaction, CancellationToken cancellationToken = default)
    {
        await _context.Paymenttransactions.AddAsync(transaction, cancellationToken);
    }

    public void Update(Paymenttransaction transaction)
    {
        _context.Paymenttransactions.Update(transaction);
    }
}
