using GymKitten.Domain.Common;

namespace GymKitten.Domain.Errors;

public static class ProductErrors
{
    public static readonly Error NotFound = new(
        "Product.NotFound",
        "The specified product was not found.");

    public static readonly Error SlugAlreadyExists = new(
        "Product.SlugAlreadyExists",
        "A product with this slug already exists.");

    public static readonly Error CategoryNotFound = new(
        "Product.CategoryNotFound",
        "The specified category was not found.");

    public static readonly Error ImageNotFound = new(
        "ProductImage.NotFound",
        "The specified product image was not found.");

    public static readonly Error UploadFailed = new(
        "ProductImage.UploadFailed",
        "Failed to upload product images.");

    public static readonly Error InactiveOrUnavailable = new(
        "Product.InactiveOrUnavailable",
        "Sản phẩm tạm dừng kinh doanh hoặc không khả dụng.");
}
