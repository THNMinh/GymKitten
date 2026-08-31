namespace GymKitten.Application.Features.Admin.Orders.Queries.GetAdminOrders;

public sealed record GetAdminOrdersResponse(
    List<AdminOrderSummaryDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public sealed record AdminOrderSummaryDto(
    Guid OrderId,
    string OrderCode,
    string CustomerEmail,
    decimal TotalAmount,
    string CurrentStatus,
    string PaymentMethod,
    string PaymentStatus,
    DateTime CreatedAt,
    int TotalItems);
