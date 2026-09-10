using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Admin.Inventory.Queries.GetInventoryTransactions;

public sealed record GetInventoryTransactionsQuery(
    Guid? VariantId = null,
    string? Sku = null,
    string? Type = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Page = 1,
    int PageSize = 10) : IQuery<Result<GetInventoryTransactionsResponse>>;
