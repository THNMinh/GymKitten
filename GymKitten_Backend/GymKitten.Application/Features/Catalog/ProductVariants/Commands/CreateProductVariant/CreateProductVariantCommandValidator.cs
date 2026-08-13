using FluentValidation;

namespace GymKitten.Application.Features.Catalog.ProductVariants.Commands.CreateProductVariant;

public sealed class CreateProductVariantCommandValidator : AbstractValidator<CreateProductVariantCommand>
{
    public CreateProductVariantCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required.");

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU is required.")
            .MaximumLength(50).WithMessage("SKU must not exceed 50 characters.");

        RuleFor(x => x.ColorName)
            .NotEmpty().WithMessage("ColorName is required.")
            .MaximumLength(50).WithMessage("ColorName must not exceed 50 characters.");

        RuleFor(x => x.Size)
            .NotEmpty().WithMessage("Size is required.")
            .MaximumLength(20).WithMessage("Size must not exceed 20 characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.");
    }
}
