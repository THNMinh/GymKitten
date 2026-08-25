using GymKitten.Domain.Common;

namespace GymKitten.Domain.Errors;

public static class PaymentErrors
{
    public static readonly Error TransactionNotFound = new(
        "Payment.TransactionNotFound",
        "The payment transaction was not found.");

    public static readonly Error InvalidAmount = new(
        "Payment.InvalidAmount",
        "Callback payment amount does not match transaction amount.");

    public static readonly Error CallbackFailed = new(
        "Payment.CallbackFailed",
        "Payment processing failed at gateway.");

    public static readonly Error InvalidSignature = new(
        "Payment.InvalidSignature",
        "Payment gateway IPN signature verification failed.");
}
