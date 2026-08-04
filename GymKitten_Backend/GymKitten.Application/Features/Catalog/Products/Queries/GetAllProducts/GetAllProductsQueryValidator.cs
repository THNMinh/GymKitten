using FluentValidation;

namespace GymKitten.Application.Features.Catalog.Products.Queries.GetAllProducts;

public sealed class GetAllProductsQueryValidator : AbstractValidator<GetAllProductsQuery>
{
    public GetAllProductsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}
