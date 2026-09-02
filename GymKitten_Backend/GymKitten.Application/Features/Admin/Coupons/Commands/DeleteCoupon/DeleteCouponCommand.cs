using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Admin.Coupons.Commands.DeleteCoupon;

public sealed record DeleteCouponCommand(Guid CouponId) : ICommand<Result>;
