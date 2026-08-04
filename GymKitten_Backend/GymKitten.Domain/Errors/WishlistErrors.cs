using GymKitten.Domain.Common;

namespace GymKitten.Domain.Errors;

public static class WishlistErrors
{
    public static readonly Error NotFound = new(
        "Wishlist.NotFound",
        "The product was not found in your wishlist.");

    public static readonly Error ProductNotFound = new(
        "Wishlist.ProductNotFound",
        "The specified product was not found or is inactive.");

    public static readonly Error Unauthorized = new(
        "Wishlist.Unauthorized",
        "User is not authenticated.");
}
