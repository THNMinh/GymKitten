using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Admin.Inventory.Queries.GetInventory;

public sealed class GetInventoryQueryHandler
    : IQueryHandler<GetInventoryQuery, Result<GetInventoryResponse>>
{
    private readonly IUserContext _userContext;
    private readonly IInventoryRepository _inventoryRepository;

    public GetInventoryQueryHandler(
        IUserContext userContext,
        IInventoryRepository inventoryRepository)
    {
        _userContext = userContext;
        _inventoryRepository = inventoryRepository;
    }

    public async Task<Result<GetInventoryResponse>> Handle(
        GetInventoryQuery request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(_userContext.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<GetInventoryResponse>(UserErrors.Forbidden);
        }

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var (items, totalCount) = await _inventoryRepository.SearchInventoryAsync(
            request.Sku,
            request.ProductName,
            page,
            pageSize,
            cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var response = new GetInventoryResponse(
            items.ToList(),
            totalCount,
            page,
            pageSize,
            totalPages);

        return Result.Success(response);
    }
}
