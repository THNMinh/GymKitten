using FluentValidation;

namespace GymKitten.Application.Features.Order.Commands.Checkout;

public sealed class CheckoutCommandValidator : AbstractValidator<CheckoutCommand>
{
    public CheckoutCommandValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Checkout items list cannot be empty.");

        RuleForEach(x => x.Items).ChildRules(items =>
        {
            items.RuleFor(i => i.VariantId)
                .NotEmpty().WithMessage("VariantId is required.");

            items.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
        });

        RuleFor(x => x.ShippingAddress)
            .NotEmpty().WithMessage("Shipping address is required.")
            .MaximumLength(500).WithMessage("Shipping address cannot exceed 500 characters.");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Payment method is required.")
            .Must(m => m.Equals("COD", StringComparison.OrdinalIgnoreCase) ||
                       m.Equals("VNPAY", StringComparison.OrdinalIgnoreCase) ||
                       m.Equals("MOMO", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Payment method must be 'COD', 'VNPAY', or 'MOMO'.");
    }
}
