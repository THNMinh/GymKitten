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
}
