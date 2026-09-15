using GymKitten.Domain.Common;

namespace GymKitten.Domain.Errors;

public static class OrderErrors
{
    public static readonly Error NotFound = new(
        "Order.NotFound",
        "The specified order was not found.");

    public static readonly Error EmptyCart = new(
        "Order.EmptyCart",
        "Checkout items list cannot be empty.");

    public static readonly Error InsufficientStock = new(
        "Order.InsufficientStock",
        "One or more items do not have sufficient stock available.");

    public static readonly Error PaymentMethodNotSupported = new(
        "Order.PaymentMethodNotSupported",
        "The selected payment method is not supported.");

    public static readonly Error AlreadyPaid = new(
        "Order.AlreadyPaid",
        "The order has already been paid.");

    public static readonly Error InvalidStatus = new(
        "Order.InvalidStatus",
        "The order status is invalid for this operation.");

    public static readonly Error AlreadyShipped = new(
        "Order.AlreadyShipped",
        "The order has already been shipped.");

    public static readonly Error CannotCancelNonPendingOrder = new(
        "Order.CannotCancelNonPendingOrder",
        "Only pending orders can be cancelled by the customer.");

    public static readonly Error AccessDenied = new(
        "Order.AccessDenied",
        "You do not have permission to access or modify this order.");

    public static readonly Error AlreadyCancelled = new(
        "Order.AlreadyCancelled",
        "Đơn hàng này đã bị hủy, không thể thay đổi trạng thái nữa.");
}
