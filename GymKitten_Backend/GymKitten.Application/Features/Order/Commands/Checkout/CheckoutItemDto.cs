namespace GymKitten.Application.Features.Order.Commands.Checkout;

public sealed record CheckoutItemDto(
    Guid VariantId,
    int Quantity);
