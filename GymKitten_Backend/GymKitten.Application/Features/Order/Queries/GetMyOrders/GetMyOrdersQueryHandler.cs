using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Features.Order.Queries.GetOrderById;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Order.Queries.GetMyOrders;

public sealed class GetMyOrdersQueryHandler
    : IQueryHandler<GetMyOrdersQuery, Result<GetMyOrdersResponse>>
{
    private readonly IUserContext _userContext;
    private readonly IOrderRepository _orderRepository;

    public GetMyOrdersQueryHandler(
        IUserContext userContext,
        IOrderRepository orderRepository)
    {
        _userContext = userContext;
        _orderRepository = orderRepository;
    }

    public async Task<Result<GetMyOrdersResponse>> Handle(
        GetMyOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        if (!_userContext.UserId.HasValue)
        {
            return Result.Success(new GetMyOrdersResponse(new List<OrderSummaryDto>(), 0, page, pageSize, 0));
        }

        var (items, totalCount) = await _orderRepository.GetMyOrdersPagedAsync(
            _userContext.UserId.Value,
            request.Status,
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
                    i.Sku,
                    i.Productname,
                    i.Unitprice,
                    i.Quantity,
                    i.Totalprice,
                    variantImage);
            }).ToList();

            return new OrderSummaryDto(
                o.Orderid,
                o.Ordercode,
                o.Totalamount,
                o.Currentstatus,
                o.Paymentmethod,
                o.Paymentstatus,
                o.Createdat,
                o.Orderitems.Count,
                orderItemDtos);
        }).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var response = new GetMyOrdersResponse(
            dtos,
            totalCount,
            page,
            pageSize,
            totalPages);

        return Result.Success(response);
    }
}
