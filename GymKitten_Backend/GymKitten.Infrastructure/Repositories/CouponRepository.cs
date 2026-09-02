using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymKitten.Infrastructure.Repositories;

public sealed class CouponRepository : ICouponRepository
{
    private readonly GymkittenContext _context;

    public CouponRepository(GymkittenContext context)
    {
        _context = context;
    }

    public async Task<Coupon?> GetByIdAsync(Guid couponId, CancellationToken cancellationToken = default)
    {
        return await _context.Coupons
            .FirstOrDefaultAsync(c => c.Couponid == couponId, cancellationToken);
    }

    public async Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;

        var normalizedCode = code.Trim().ToUpper();
        return await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == normalizedCode, cancellationToken);
    }

    public async Task<List<Coupon>> GetActiveCouponsAsync(DateTime now, CancellationToken cancellationToken = default)
    {
        return await _context.Coupons
            .AsNoTracking()
            .Where(c => c.Deletedat == null
                        && c.Isactive
                        && c.Startdate <= now
                        && c.Enddate >= now
                        && (c.Usagelimit == null || c.Usedcount < c.Usagelimit))
            .OrderByDescending(c => c.Createdat)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Coupon> Items, int TotalCount)> SearchAdminCouponsAsync(
        string? code,
        string? discountType,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Coupons
            .AsNoTracking()
            .Where(c => c.Deletedat == null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(code))
        {
            var codeTerm = code.Trim().ToUpper();
            query = query.Where(c => c.Code.ToUpper().Contains(codeTerm));
        }

        if (!string.IsNullOrWhiteSpace(discountType))
        {
            var typeTerm = discountType.Trim();
            query = query.Where(c => c.Discounttype.ToLower() == typeTerm.ToLower());
        }

        if (isActive.HasValue)
        {
            query = query.Where(c => c.Isactive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.Createdat)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code)) return false;

        var normalizedCode = code.Trim().ToUpper();
        return await _context.Coupons
            .AsNoTracking()
            .AnyAsync(c => c.Deletedat == null && c.Code.ToUpper() == normalizedCode, cancellationToken);
    }

    public async Task<bool> ExistsByCodeExcludingIdAsync(string code, Guid couponId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code)) return false;

        var normalizedCode = code.Trim().ToUpper();
        return await _context.Coupons
            .AsNoTracking()
            .AnyAsync(c => c.Deletedat == null && c.Couponid != couponId && c.Code.ToUpper() == normalizedCode, cancellationToken);
    }

    public async Task AddAsync(Coupon coupon, CancellationToken cancellationToken = default)
    {
        await _context.Coupons.AddAsync(coupon, cancellationToken);
    }

    public async Task AddUsageAsync(Couponusage couponUsage, CancellationToken cancellationToken = default)
    {
        await _context.Couponusages.AddAsync(couponUsage, cancellationToken);
    }

    public void Update(Coupon coupon)
    {
        _context.Coupons.Update(coupon);
    }
}
