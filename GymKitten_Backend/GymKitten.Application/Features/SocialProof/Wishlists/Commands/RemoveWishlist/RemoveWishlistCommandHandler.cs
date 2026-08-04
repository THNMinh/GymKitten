using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.SocialProof.Wishlists.Commands.RemoveWishlist;

public sealed class RemoveWishlistCommandHandler
    : ICommandHandler<RemoveWishlistCommand, Result>
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IUserContext _userContext;

    public RemoveWishlistCommandHandler(
        IWishlistRepository wishlistRepository,
        IUserContext userContext)
    {
        _wishlistRepository = wishlistRepository;
        _userContext = userContext;
    }

    public async Task<Result> Handle(
        RemoveWishlistCommand request,
        CancellationToken cancellationToken)
    {
        if (!_userContext.IsAuthenticated || !_userContext.UserId.HasValue)
        {
            return Result.Failure(WishlistErrors.Unauthorized);
        }

        // Direct bulk hard delete via Repository (bypassing Soft Delete Interceptor)
        var deletedRows = await _wishlistRepository.ExecuteHardDeleteAsync(
            _userContext.UserId.Value,
            request.ProductId,
            cancellationToken);

        if (deletedRows == 0)
        {
            return Result.Failure(WishlistErrors.NotFound);
        }

        return Result.Success();
    }
}
