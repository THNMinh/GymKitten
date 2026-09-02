using GymKitten.Domain.Common;

namespace GymKitten.Domain.Errors;

public static class CouponErrors
{
    public static readonly Error NotFound = new(
        "Coupon.NotFound",
        "The specified coupon code was not found.");

    public static readonly Error Expired = new(
        "Coupon.Expired",
        "This coupon code has expired or is not active yet.");

    public static readonly Error Inactive = new(
        "Coupon.Inactive",
        "This coupon code is currently disabled.");

    public static readonly Error UsageLimitReached = new(
        "Coupon.UsageLimitReached",
        "This coupon code has reached its maximum usage limit.");

    public static readonly Error MinOrderValueNotMet = new(
        "Coupon.MinOrderValueNotMet",
        "The order subtotal does not meet the minimum required value for this coupon.");

    public static readonly Error CodeAlreadyExists = new(
        "Coupon.CodeAlreadyExists",
        "A coupon with this code already exists.");

    public static readonly Error InvalidDiscountType = new(
        "Coupon.InvalidDiscountType",
        "Discount type must be either 'Percentage' or 'FixedAmount'.");
}
