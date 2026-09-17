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

        var itemList = items.ToList();

        var groupedProducts = itemList
            .GroupBy(i => new { i.ProductId, i.ProductName })
            .Select(p =>
            {
                var colorGroups = p
                    .GroupBy(c => new { ColorName = c.Color.Trim(), c.ColorHex })
                    .Select(cg => new ColorInventoryGroupDto(
                        Color: cg.Key.ColorName,
                        ColorHex: cg.Key.ColorHex,
                        QuantityOnHand: cg.Sum(s => s.QuantityOnHand),
                        QuantityReserved: cg.Sum(s => s.QuantityReserved),
                        AvailableStock: cg.Sum(s => s.AvailableStock),
                        Sizes: cg.OrderBy(s => s.Size).ToList(),
                        ColorName: cg.Key.ColorName
                    ))
                    .OrderBy(cg => cg.Color)
                    .ToList();

                return new ProductInventoryGroupDto(
                    p.Key.ProductId,
                    p.Key.ProductName,
                    p.Sum(i => i.QuantityOnHand),
                    p.Sum(i => i.QuantityReserved),
                    p.Sum(i => i.AvailableStock),
                    p.Count(),
                    colorGroups
                );
            })
            .OrderBy(p => p.ProductName)
            .ToList();

        var response = new GetInventoryResponse(
            itemList,
            totalCount,
            page,
            pageSize,
            totalPages,
            groupedProducts);

        return Result.Success(response);
    }
}
