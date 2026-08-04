using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.SocialProof.Wishlists.Commands.ToggleWishlist;

public sealed class ToggleWishlistCommandHandler
    : ICommandHandler<ToggleWishlistCommand, Result<ToggleWishlistResponse>>
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserContext _userContext;

    public ToggleWishlistCommandHandler(
        IWishlistRepository wishlistRepository,
        IProductRepository productRepository,
        IUserContext userContext)
    {
        _wishlistRepository = wishlistRepository;
        _productRepository = productRepository;
        _userContext = userContext;
    }

    public async Task<Result<ToggleWishlistResponse>> Handle(
        ToggleWishlistCommand request,
        CancellationToken cancellationToken)
    {
        if (!_userContext.IsAuthenticated || !_userContext.UserId.HasValue)
        {
            return Result.Failure<ToggleWishlistResponse>(WishlistErrors.Unauthorized);
        }

        // 1. Verify product exists and is active
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null || !product.Isactive)
        {
            return Result.Failure<ToggleWishlistResponse>(WishlistErrors.ProductNotFound);
        }

        // 2. Toggle wishlist state
        var isAdded = await _wishlistRepository.ToggleWishlistAsync(
            _userContext.UserId.Value,
            request.ProductId,
            cancellationToken);

        var message = isAdded
            ? "Product added to wishlist."
            : "Product removed from wishlist.";

        return Result.Success(new ToggleWishlistResponse(isAdded, message));
    }
}
