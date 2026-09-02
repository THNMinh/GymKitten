using GymKitten.Domain.Entities;

namespace GymKitten.Application.Abstractions.Repositories;

public interface ICouponRepository
{
    Task<Coupon?> GetByIdAsync(Guid couponId, CancellationToken cancellationToken = default);

    Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<List<Coupon>> GetActiveCouponsAsync(DateTime now, CancellationToken cancellationToken = default);

    Task<(IEnumerable<Coupon> Items, int TotalCount)> SearchAdminCouponsAsync(
        string? code,
        string? discountType,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<bool> ExistsByCodeExcludingIdAsync(string code, Guid couponId, CancellationToken cancellationToken = default);

    Task AddAsync(Coupon coupon, CancellationToken cancellationToken = default);

    Task AddUsageAsync(Couponusage couponUsage, CancellationToken cancellationToken = default);

    void Update(Coupon coupon);
}
