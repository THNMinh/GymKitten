using FluentValidation;

namespace GymKitten.Application.Features.Admin.Inventory.Commands.Restock;

public sealed class RestockCommandValidator : AbstractValidator<RestockCommand>
{
    public RestockCommandValidator()
    {
        RuleFor(x => x.VariantId)
            .NotEmpty().WithMessage("VariantId is required.");

        RuleFor(x => x.QuantityAdded)
            .GreaterThan(0).WithMessage("QuantityAdded must be greater than 0.");

        RuleFor(x => x.Note)
            .NotEmpty().WithMessage("Note is required.")
            .MaximumLength(500).WithMessage("Note cannot exceed 500 characters.");
    }
}
