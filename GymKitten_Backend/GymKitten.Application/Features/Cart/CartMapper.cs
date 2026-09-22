using GymKitten.Application.Features.Cart.DTOs;
using GymKitten.Domain.Entities;

namespace GymKitten.Application.Features.Cart;

public static class CartMapper
{
    public static CartDto ToDto(Domain.Entities.Cart cart)
    {
        var items = (cart.Cartitems ?? new List<Cartitem>())
            .Where(ci => ci.Deletedat == null)
            .Select(ci =>
            {
                var variant = ci.Variant;
                var product = variant?.Product;
                var inventory = variant?.Inventoryitem;
                var availableStock = inventory != null
                    ? Math.Max(0, inventory.Quantityonhand - inventory.Quantityreserved)
                    : 0;

                var isAvailable = product != null &&
                                  product.Isactive &&
                                  product.Deletedat == null &&
                                  availableStock >= ci.Quantity;

                var imageUrl = variant?.Productimages.FirstOrDefault()?.Imageurl
                    ?? product?.Productimages.FirstOrDefault(img => img.Variantid == variant?.Variantid)?.Imageurl
                    ?? product?.Productimages.FirstOrDefault(img => img.Isprimary)?.Imageurl
                    ?? product?.Productimages.FirstOrDefault()?.Imageurl;

                return new CartItemDto(
                    ci.Cartitemid,
                    ci.Variantid,
                    product?.Productid ?? Guid.Empty,
                    product?.Name ?? string.Empty,
                    variant?.Colorname ?? string.Empty,
                    variant?.Colorhex,
                    variant?.Size ?? string.Empty,
                    imageUrl,
                    variant?.Price ?? 0,
                    variant?.Originalprice,
                    ci.Quantity,
                    availableStock,
                    isAvailable);
            })
            .ToList();

        var totalItems = items.Sum(i => i.Quantity);
        var subTotal = items.Sum(i => i.Price * i.Quantity);

        return new CartDto(cart.Cartid, totalItems, subTotal, items);
    }
}
