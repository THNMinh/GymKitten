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

    public static readonly Error InvalidPrice = new(
        "ProductVariant.InvalidPrice",
        "Giá sản phẩm vượt quá giới hạn cho phép (tối đa 1.000.000.000đ).");

    public static readonly Error DuplicateColorAndSize = new(
        "ProductVariant.DuplicateColorAndSize",
        "Sản phẩm đã có biến thể với màu sắc và kích cỡ này.");
}
