using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Admin.Inventory.Queries.GetInventoryTransactions;

public sealed class GetInventoryTransactionsQueryHandler
    : IQueryHandler<GetInventoryTransactionsQuery, Result<GetInventoryTransactionsResponse>>
{
    private readonly IUserContext _userContext;
    private readonly IInventoryTransactionRepository _inventoryTransactionRepository;

    public GetInventoryTransactionsQueryHandler(
        IUserContext userContext,
        IInventoryTransactionRepository inventoryTransactionRepository)
    {
        _userContext = userContext;
        _inventoryTransactionRepository = inventoryTransactionRepository;
    }

    public async Task<Result<GetInventoryTransactionsResponse>> Handle(
        GetInventoryTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(_userContext.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<GetInventoryTransactionsResponse>(InventoryTransactionErrors.Forbidden);
        }

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        var (items, totalCount) = await _inventoryTransactionRepository.SearchTransactionsAsync(
            request.VariantId,
            request.Sku,
            request.Type,
            request.FromDate,
            request.ToDate,
            page,
            pageSize,
            cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var dtos = items.Select(t => new InventoryTransactionDto(
            t.Transactionid,
            t.Variantid,
            t.Variant?.Sku ?? "N/A",
            t.Variant?.Product?.Name ?? "Unknown Product",
            t.Variant?.Colorname ?? string.Empty,
            t.Variant?.Size ?? string.Empty,
            t.Quantitychange,
            t.Type,
            t.Referenceid,
            ExtractPerformer(t.Referenceid),
            t.Createdat
        ));

        return Result.Success(new GetInventoryTransactionsResponse(
            dtos,
            totalCount,
            page,
            pageSize,
            totalPages));
    }

    private static string ExtractPerformer(string? referenceId)
    {
        if (string.IsNullOrWhiteSpace(referenceId))
        {
            return "System";
        }

        var idxBy = referenceId.LastIndexOf("| By: ", StringComparison.OrdinalIgnoreCase);
        if (idxBy >= 0)
        {
            return referenceId[(idxBy + 6)..].Trim();
        }

        var idxAdmin = referenceId.LastIndexOf("| Admin: ", StringComparison.OrdinalIgnoreCase);
        if (idxAdmin >= 0)
        {
            return referenceId[(idxAdmin + 9)..].Trim();
        }

        var idxCustomer = referenceId.LastIndexOf("| Customer: ", StringComparison.OrdinalIgnoreCase);
        if (idxCustomer >= 0)
        {
            return referenceId[(idxCustomer + 12)..].Trim();
        }

        if (referenceId.StartsWith("By: ", StringComparison.OrdinalIgnoreCase))
        {
            return referenceId[4..].Trim();
        }

        return referenceId;
    }
}
