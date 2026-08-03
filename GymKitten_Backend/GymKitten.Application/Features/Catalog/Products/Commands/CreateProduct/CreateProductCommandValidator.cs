using FluentValidation;

namespace GymKitten.Application.Features.Catalog.Products.Commands.CreateProduct;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("CategoryId is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .MaximumLength(250).WithMessage("Slug cannot exceed 250 characters.");

        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage("Gender is required.")
            .MaximumLength(20).WithMessage("Gender cannot exceed 20 characters.");
    }
}
