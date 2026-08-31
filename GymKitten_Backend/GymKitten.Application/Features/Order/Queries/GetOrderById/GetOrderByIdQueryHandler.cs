using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Order.Queries.GetOrderById;

public sealed class GetOrderByIdQueryHandler
    : IQueryHandler<GetOrderByIdQuery, Result<OrderDetailDto>>
{
    private readonly IUserContext _userContext;
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdQueryHandler(
        IUserContext userContext,
        IOrderRepository orderRepository)
    {
        _userContext = userContext;
        _orderRepository = orderRepository;
    }

    public async Task<Result<OrderDetailDto>> Handle(
        GetOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure<OrderDetailDto>(OrderErrors.NotFound);
        }

        // Security check: verify ownership if not admin/system
        if (order.Userid.HasValue && order.Userid != _userContext.UserId)
        {
            return Result.Failure<OrderDetailDto>(OrderErrors.AccessDenied);
        }

        var items = order.Orderitems.Select(i => new OrderItemDto(
            i.Orderitemid,
            i.Variantid,
            i.Sku,
            i.Productname,
            i.Unitprice,
            i.Quantity,
            i.Totalprice)).ToList();

        var dto = new OrderDetailDto(
            order.Orderid,
            order.Ordercode,
            order.Userid,
            order.Shippingaddress,
            order.Subtotal,
            order.Shippingfee,
            order.Discountamount,
            order.Totalamount,
            order.Currentstatus,
            order.Paymentmethod,
            order.Paymentstatus,
            order.Customernote,
            order.Createdat,
            order.Updatedat,
            items);

        return Result.Success(dto);
    }
}
