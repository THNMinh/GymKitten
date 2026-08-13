using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.SocialProof.Wishlists.Queries.GetMyWishlist;

public sealed class GetMyWishlistQueryHandler
    : IQueryHandler<GetMyWishlistQuery, Result<GetMyWishlistResponse>>
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IUserContext _userContext;

    public GetMyWishlistQueryHandler(
        IWishlistRepository wishlistRepository,
        IUserContext userContext)
    {
        _wishlistRepository = wishlistRepository;
        _userContext = userContext;
    }

    public async Task<Result<GetMyWishlistResponse>> Handle(
        GetMyWishlistQuery request,
        CancellationToken cancellationToken)
    {
        if (!_userContext.IsAuthenticated || !_userContext.UserId.HasValue)
        {
            return Result.Failure<GetMyWishlistResponse>(WishlistErrors.Unauthorized);
        }

        // Count First, Take Later via Repository
        var (wishlists, total) = await _wishlistRepository.GetWishlistByUserIdAsync(
            _userContext.UserId.Value,
            request.Page,
            request.PageSize,
            cancellationToken);

        var items = wishlists.Select(w =>
        {
            var primaryImage = w.Product.Productimages
                .OrderByDescending(img => img.Variantid == null)
                .ThenByDescending(img => img.Isprimary)
                .ThenBy(img => img.Displayorder)
                .FirstOrDefault();

            return new WishlistItemDto(
                w.Productid,
                w.Product.Name,
                w.Product.Slug,
                w.Product.Productvariants.Any() ? w.Product.Productvariants.Min(v => v.Price) : 0m,
                primaryImage?.Imageurl,
                w.Createdat);
        }).ToList();

        var totalPages = (int)Math.Ceiling(total / (double)request.PageSize);

        var response = new GetMyWishlistResponse(
            items,
            total,
            request.Page,
            request.PageSize,
            totalPages);

        return Result.Success(response);
    }
}
