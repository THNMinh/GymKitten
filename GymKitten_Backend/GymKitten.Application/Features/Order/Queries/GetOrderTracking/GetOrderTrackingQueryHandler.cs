using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Order.Queries.GetOrderTracking;

public sealed class GetOrderTrackingQueryHandler
    : IQueryHandler<GetOrderTrackingQuery, Result<List<OrderTrackingDto>>>
{
    private readonly IUserContext _userContext;
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderTrackingRepository _orderTrackingRepository;

    public GetOrderTrackingQueryHandler(
        IUserContext userContext,
        IOrderRepository orderRepository,
        IOrderTrackingRepository orderTrackingRepository)
    {
        _userContext = userContext;
        _orderRepository = orderRepository;
        _orderTrackingRepository = orderTrackingRepository;
    }

    public async Task<Result<List<OrderTrackingDto>>> Handle(
        GetOrderTrackingQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure<List<OrderTrackingDto>>(OrderErrors.NotFound);
        }

        // Verify ownership if user order
        if (order.Userid.HasValue && order.Userid != _userContext.UserId)
        {
            return Result.Failure<List<OrderTrackingDto>>(OrderErrors.AccessDenied);
        }

        var trackingList = await _orderTrackingRepository.GetTrackingHistoryByOrderIdAsync(request.OrderId, cancellationToken);

        var dtos = trackingList.Select(t => new OrderTrackingDto(
            t.Trackingid,
            t.Orderid,
            t.Status,
            t.Title,
            t.Description,
            t.Location,
            t.Timestamp,
            t.Createdat)).ToList();

        return Result.Success(dtos);
    }
}
