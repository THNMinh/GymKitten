using GymKitten.Application.Abstractions.Repositories;
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

    public async Task AddAsync(Paymenttransaction transaction, CancellationToken cancellationToken = default)
    {
        await _context.Paymenttransactions.AddAsync(transaction, cancellationToken);
    }

    public void Update(Paymenttransaction transaction)
    {
        _context.Paymenttransactions.Update(transaction);
    }
}
