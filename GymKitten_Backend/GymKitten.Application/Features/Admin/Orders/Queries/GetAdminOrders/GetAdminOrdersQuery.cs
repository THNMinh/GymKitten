using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Admin.Orders.Queries.GetAdminOrders;

public sealed record GetAdminOrdersQuery(
    string? OrderCode = null,
    string? Status = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int Page = 1,
    int PageSize = 20) : IQuery<Result<GetAdminOrdersResponse>>;
