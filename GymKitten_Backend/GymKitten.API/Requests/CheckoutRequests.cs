namespace GymKitten.API.Requests;

public sealed record CheckoutItemRequest(
    Guid VariantId,
    int Quantity);

public sealed record CheckoutRequest(
    List<CheckoutItemRequest> Items,
    string ShippingAddress,
    string PaymentMethod,
    string? CustomerNote,
    string? CouponCode = null);
