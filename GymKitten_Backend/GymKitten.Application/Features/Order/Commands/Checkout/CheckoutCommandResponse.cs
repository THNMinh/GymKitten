namespace GymKitten.Application.Features.Order.Commands.Checkout;

public sealed record CheckoutCommandResponse(
    Guid OrderId,
    string OrderCode,
    decimal TotalAmount,
    string Status,
    string PaymentStatus,
    string? PaymentUrl = null);
