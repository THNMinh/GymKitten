using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Admin.Inventory.Queries.GetInventory;

public sealed class GetInventoryQueryHandler
    : IQueryHandler<GetInventoryQuery, Result<GetInventoryResponse>>
{
    private readonly IInventoryRepository _inventoryRepository;

    public GetInventoryQueryHandler(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task<Result<GetInventoryResponse>> Handle(
        GetInventoryQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var (items, totalCount) = await _inventoryRepository.GetInventoryPagedAsync(
            request.Sku,
            request.ProductName,
            page,
            pageSize,
            cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var response = new GetInventoryResponse(
            items,
            totalCount,
            page,
            pageSize,
            totalPages);

        return Result.Success(response);
    }
}
