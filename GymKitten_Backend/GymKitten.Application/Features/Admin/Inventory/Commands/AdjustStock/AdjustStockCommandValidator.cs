using FluentValidation;

namespace GymKitten.Application.Features.Admin.Inventory.Commands.AdjustStock;

public sealed class AdjustStockCommandValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockCommandValidator()
    {
        RuleFor(x => x.VariantId)
            .NotEmpty().WithMessage("VariantId is required.");

        RuleFor(x => x.NewQuantityOnHand)
            .GreaterThanOrEqualTo(0).WithMessage("NewQuantityOnHand must be greater than or equal to 0.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters.");
    }
}
