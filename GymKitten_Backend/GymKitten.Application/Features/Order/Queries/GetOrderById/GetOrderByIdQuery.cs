using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Order.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(Guid OrderId) : IQuery<Result<OrderDetailDto>>;

public sealed record OrderDetailDto(
    Guid OrderId,
    string OrderCode,
    Guid? UserId,
    string ShippingAddress,
    decimal Subtotal,
    decimal ShippingFee,
    decimal DiscountAmount,
    decimal TotalAmount,
    string CurrentStatus,
    string PaymentMethod,
    string PaymentStatus,
    string? CustomerNote,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<OrderItemDto> Items);

public sealed record OrderItemDto(
    Guid OrderItemId,
    Guid VariantId,
    string Sku,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice);
