using GymKitten.Domain.Common;

namespace GymKitten.Domain.Errors;

public static class ProductVariantErrors
{
    public static readonly Error NotFound = new(
        "ProductVariant.NotFound",
        "Không tìm thấy biến thể sản phẩm.");

    public static readonly Error SkuAlreadyExists = new(
        "ProductVariant.SkuAlreadyExists",
        "Mã SKU biến thể đã tồn tại trong hệ thống. Vui lòng nhập mã SKU khác.");

    public static readonly Error InvalidPrice = new(
        "ProductVariant.InvalidPrice",
        "Giá sản phẩm vượt quá giới hạn cho phép (tối đa 1.000.000.000đ).");

    public static readonly Error DuplicateColorAndSize = new(
        "ProductVariant.DuplicateColorAndSize",
        "Sản phẩm đã có biến thể với màu sắc và kích cỡ này.");

    public static readonly Error CannotDeleteWithOrders = new(
        "ProductVariant.CannotDeleteWithOrders",
        "Không thể xóa biến thể này vì đã có đơn hàng liên kết.");
}
