namespace GymKitten.Domain.Enums;

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

public static class OrderStatusExtensions
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Shipped = "Shipped";
    public const string Delivered = "Delivered";
    public const string Cancelled = "Cancelled";
}
