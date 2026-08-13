using GymKitten.Domain.Common;

namespace GymKitten.Domain.Errors;

public static class ProductVariantErrors
{
    public static readonly Error NotFound = new(
        "ProductVariant.NotFound",
        "Product variant was not found.");

    public static readonly Error SkuAlreadyExists = new(
        "ProductVariant.SkuAlreadyExists",
        "A product variant with this SKU already exists.");
}
