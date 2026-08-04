using FluentValidation;

namespace GymKitten.Application.Features.SocialProof.Wishlists.Queries.GetMyWishlist;

public sealed class GetMyWishlistQueryValidator : AbstractValidator<GetMyWishlistQuery>
{
    public GetMyWishlistQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}
