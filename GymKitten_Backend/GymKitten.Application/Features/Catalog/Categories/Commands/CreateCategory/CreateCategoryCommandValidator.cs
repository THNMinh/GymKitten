using FluentValidation;

namespace GymKitten.Application.Features.Catalog.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100).WithMessage("Category name cannot exceed 100 characters.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Category slug is required.")
            .MaximumLength(150).WithMessage("Category slug cannot exceed 150 characters.");
    }
}
