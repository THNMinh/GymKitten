using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Order.Queries.GetMyOrders;

public sealed record GetMyOrdersQuery(
    string? Status = null,
    int Page = 1,
    int PageSize = 20) : IQuery<Result<GetMyOrdersResponse>>;
