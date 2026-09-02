using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Promotions.Coupons.Queries.GetActiveCoupons;

public sealed record GetActiveCouponsQuery : IQuery<Result<GetActiveCouponsResponse>>;
