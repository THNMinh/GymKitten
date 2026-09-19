using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymKitten.Infrastructure.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly GymkittenContext _context;

    public OrderRepository(GymkittenContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .FirstOrDefaultAsync(o => o.Orderid == orderId, cancellationToken);
    }

    public async Task<Order?> GetByIdWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Orderitems)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Orderid == orderId, cancellationToken);
    }

    public async Task<Order?> GetByIdWithDetailsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.Orderitems)
                .ThenInclude(i => i.Variant)
                    .ThenInclude(v => v.Productimages)
            .Include(o => o.Ordertrackinghistories)
            .Include(o => o.Paymenttransactions)
            .FirstOrDefaultAsync(o => o.Orderid == orderId, cancellationToken);
    }

    public async Task<(IEnumerable<Order> Items, int TotalCount)> SearchMyOrdersAsync(
        Guid userId,
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Include(o => o.Orderitems)
                .ThenInclude(i => i.Variant)
                    .ThenInclude(v => v.Productimages)
            .Where(o => o.Userid == userId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var statusTerm = status.Trim().ToLower();
            query = query.Where(o => o.Currentstatus.ToLower() == statusTerm);
        }

        // Count First
        var totalCount = await query.CountAsync(cancellationToken);

        // Take Later
        var items = await query
            .OrderByDescending(o => o.Createdat)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(IEnumerable<Order> Items, int TotalCount)> SearchAdminOrdersAsync(
        string? orderCode,
        string? status,
        DateTime? startDate,
        DateTime? endDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.Orderitems)
                .ThenInclude(i => i.Variant)
                    .ThenInclude(v => v.Productimages)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(orderCode))
        {
            var codeTerm = orderCode.Trim().ToLower();
            query = query.Where(o => o.Ordercode.ToLower().Contains(codeTerm));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var statusTerm = status.Trim().ToLower();
            query = query.Where(o => o.Currentstatus.ToLower() == statusTerm);
        }

        if (startDate.HasValue)
        {
            query = query.Where(o => o.Createdat >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(o => o.Createdat <= endDate.Value);
        }

        // Count First
        var totalCount = await query.CountAsync(cancellationToken);

        // Take Later
        var items = await query
            .OrderByDescending(o => o.Createdat)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _context.Orders.AddAsync(order, cancellationToken);
    }

    public void Update(Order order)
    {
        _context.Orders.Update(order);
    }
}
