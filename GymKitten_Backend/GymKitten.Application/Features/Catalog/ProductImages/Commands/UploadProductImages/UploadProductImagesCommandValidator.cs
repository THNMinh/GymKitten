using FluentValidation;

namespace GymKitten.Application.Features.Catalog.ProductImages.Commands.UploadProductImages;

public sealed class UploadProductImagesCommandValidator : AbstractValidator<UploadProductImagesCommand>
{
    public UploadProductImagesCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required.");

        RuleFor(x => x.Photos)
            .NotNull().WithMessage("Photos collection is required.")
            .Must(photos => photos != null && photos.Count > 0).WithMessage("At least one photo must be provided.");
    }
}
