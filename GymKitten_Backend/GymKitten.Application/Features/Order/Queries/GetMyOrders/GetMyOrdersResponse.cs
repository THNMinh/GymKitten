using GymKitten.Application.Features.Order.Queries.GetOrderById;

namespace GymKitten.Application.Features.Order.Queries.GetMyOrders;

public sealed record GetMyOrdersResponse(
    List<OrderSummaryDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public sealed record OrderSummaryDto(
    Guid OrderId,
    string OrderCode,
    decimal TotalAmount,
    string CurrentStatus,
    string PaymentMethod,
    string PaymentStatus,
    DateTime CreatedAt,
    int TotalItems,
    List<OrderItemDto> Items);
