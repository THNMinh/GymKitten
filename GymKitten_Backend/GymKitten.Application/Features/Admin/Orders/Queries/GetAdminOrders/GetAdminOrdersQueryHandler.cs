using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Features.Order.Queries.GetOrderById;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Admin.Orders.Queries.GetAdminOrders;

public sealed class GetAdminOrdersQueryHandler
    : IQueryHandler<GetAdminOrdersQuery, Result<GetAdminOrdersResponse>>
{
    private readonly IUserContext _userContext;
    private readonly IOrderRepository _orderRepository;

    public GetAdminOrdersQueryHandler(
        IUserContext userContext,
        IOrderRepository orderRepository)
    {
        _userContext = userContext;
        _orderRepository = orderRepository;
    }

    public async Task<Result<GetAdminOrdersResponse>> Handle(
        GetAdminOrdersQuery request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(_userContext.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<GetAdminOrdersResponse>(UserErrors.Forbidden);
        }

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var (items, totalCount) = await _orderRepository.GetAdminOrdersPagedAsync(
            request.OrderCode,
            request.Status,
            request.StartDate,
            request.EndDate,
            page,
            pageSize,
            cancellationToken);

        var dtos = items.Select(o =>
        {
            var orderItemDtos = o.Orderitems.Select(i =>
            {
                var variantImage = i.Variant?.Productimages
                    ?.Where(img => img.Variantid == i.Variantid)
                    .OrderByDescending(img => img.Isprimary)
                    .ThenBy(img => img.Displayorder)
                    .ThenBy(img => img.Createdat)
                    .FirstOrDefault()?.Imageurl;

                return new OrderItemDto(
                    i.Orderitemid,
                    i.Variantid,
                    i.Variant?.Productid ?? Guid.Empty,
                    i.Sku,
                    i.Productname,
                    i.Unitprice,
                    i.Quantity,
                    i.Totalprice,
                    variantImage);
            }).ToList();

            return new AdminOrderSummaryDto(
                o.Orderid,
                o.Ordercode,
                o.User != null ? o.User.Email : "Guest",
                o.Totalamount,
                o.Currentstatus,
                o.Paymentmethod,
                o.Paymentstatus,
                o.Createdat,
                o.Orderitems.Count,
                orderItemDtos);
        }).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var response = new GetAdminOrdersResponse(
            dtos,
            totalCount,
            page,
            pageSize,
            totalPages);

        return Result.Success(response);
    }
}
