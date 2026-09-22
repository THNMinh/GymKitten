using GymKitten.Domain.Common;

namespace GymKitten.Domain.Errors;

public static class CartErrors
{
    public static readonly Error Unauthorized = new(
        "Cart.Unauthorized",
        "User is not authenticated.");

    public static readonly Error CartNotFound = new(
        "Cart.NotFound",
        "The cart was not found.");

    public static readonly Error ItemNotFound = new(
        "Cart.ItemNotFound",
        "The specified item was not found in your cart.");

    public static readonly Error InvalidQuantity = new(
        "Cart.InvalidQuantity",
        "Quantity must be greater than 0.");

    public static readonly Error VariantNotFound = new(
        "Cart.VariantNotFound",
        "The specified product variant was not found.");

    public static readonly Error ProductInactive = new(
        "Cart.ProductInactive",
        "The product is inactive or unavailable for purchase.");

    public static readonly Error InsufficientStock = new(
        "Cart.InsufficientStock",
        "The requested quantity exceeds available inventory stock.");
}
