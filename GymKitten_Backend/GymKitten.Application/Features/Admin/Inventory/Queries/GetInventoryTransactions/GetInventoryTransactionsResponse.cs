namespace GymKitten.Application.Features.Admin.Inventory.Queries.GetInventoryTransactions;

public sealed record InventoryTransactionDto(
    Guid TransactionId,
    Guid VariantId,
    string Sku,
    string ProductName,
    string ColorName,
    string Size,
    int QuantityChange,
    string Type,
    string? ReferenceId,
    string Performer,
    DateTime CreatedAt);

public sealed record GetInventoryTransactionsResponse(
    IEnumerable<InventoryTransactionDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
