using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymKitten.Infrastructure.Repositories;

public sealed class OrderTrackingRepository : IOrderTrackingRepository
{
    private readonly GymkittenContext _context;

    public OrderTrackingRepository(GymkittenContext context)
    {
        _context = context;
    }

    public async Task<List<Ordertrackinghistory>> GetTrackingHistoryByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Ordertrackinghistories
            .AsNoTracking()
            .Where(t => t.Orderid == orderId)
            .OrderBy(t => t.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Ordertrackinghistory trackingHistory,
        CancellationToken cancellationToken = default)
    {
        await _context.Ordertrackinghistories.AddAsync(trackingHistory, cancellationToken);
    }
}
