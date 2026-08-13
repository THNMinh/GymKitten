using FluentValidation;

namespace GymKitten.Application.Features.Catalog.ProductVariants.Commands.UpdateProductVariant;

public sealed class UpdateProductVariantCommandValidator : AbstractValidator<UpdateProductVariantCommand>
{
    public UpdateProductVariantCommandValidator()
    {
        When(x => !string.IsNullOrWhiteSpace(x.Sku), () =>
        {
            RuleFor(x => x.Sku)
                .MaximumLength(50).WithMessage("SKU must not exceed 50 characters.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.ColorName), () =>
        {
            RuleFor(x => x.ColorName)
                .MaximumLength(50).WithMessage("ColorName must not exceed 50 characters.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Size), () =>
        {
            RuleFor(x => x.Size)
                .MaximumLength(20).WithMessage("Size must not exceed 20 characters.");
        });

        When(x => x.Price.HasValue, () =>
        {
            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.");
        });
    }
}
