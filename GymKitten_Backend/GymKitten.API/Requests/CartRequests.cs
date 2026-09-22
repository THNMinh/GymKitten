namespace GymKitten.API.Requests;

public sealed record AddToCartRequest(Guid VariantId, int Quantity = 1);

public sealed record UpdateCartItemRequest(int Quantity);
