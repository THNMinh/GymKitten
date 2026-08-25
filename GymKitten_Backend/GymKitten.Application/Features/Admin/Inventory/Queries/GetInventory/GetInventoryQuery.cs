using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Admin.Inventory.Queries.GetInventory;

public sealed record GetInventoryQuery(
    string? Sku = null,
    string? ProductName = null,
    int Page = 1,
    int PageSize = 20) : IQuery<Result<GetInventoryResponse>>;
