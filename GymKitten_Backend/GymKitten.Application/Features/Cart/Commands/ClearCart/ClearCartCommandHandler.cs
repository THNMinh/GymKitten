using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Cart.Commands.ClearCart;

public sealed class ClearCartCommandHandler : ICommandHandler<ClearCartCommand, Result>
{
    private readonly ICartRepository _cartRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public ClearCartCommandHandler(
        ICartRepository cartRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        if (!_userContext.IsAuthenticated || !_userContext.UserId.HasValue)
        {
            return Result.Failure(CartErrors.Unauthorized);
        }

        var userId = _userContext.UserId.Value;
        var cart = await _cartRepository.GetActiveCartByUserIdAsync(userId, cancellationToken);
        if (cart is null)
        {
            return Result.Success();
        }

        await _cartRepository.ClearCartItemsAsync(cart.Cartid, cancellationToken);
        cart.Updatedat = DateTime.UtcNow;
        _cartRepository.UpdateCart(cart);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
