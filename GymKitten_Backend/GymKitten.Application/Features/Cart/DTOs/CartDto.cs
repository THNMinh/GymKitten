namespace GymKitten.Application.Features.Cart.DTOs;

public sealed record CartDto(
    Guid CartId,
    int TotalItems,
    decimal SubTotal,
    List<CartItemDto> Items);

public sealed record CartItemDto(
    Guid CartItemId,
    Guid VariantId,
    Guid ProductId,
    string ProductName,
    string ColorName,
    string? ColorHex,
    string Size,
    string? ImageUrl,
    decimal Price,
    decimal? OriginalPrice,
    int Quantity,
    int AvailableStock,
    bool IsAvailable);
