using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Admin.Coupons.Queries.GetAdminCoupons;

public sealed record GetAdminCouponsQuery(
    string? Code = null,
    string? DiscountType = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 10) : IQuery<Result<GetAdminCouponsResponse>>;
