using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Features.Cart.DTOs;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Cart.Commands.RemoveCartItem;

public sealed class RemoveCartItemCommandHandler : ICommandHandler<RemoveCartItemCommand, Result<CartDto>>
{
    private readonly ICartRepository _cartRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveCartItemCommandHandler(
        ICartRepository cartRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CartDto>> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        if (!_userContext.IsAuthenticated || !_userContext.UserId.HasValue)
        {
            return Result.Failure<CartDto>(CartErrors.Unauthorized);
        }

        var userId = _userContext.UserId.Value;
        var cart = await _cartRepository.GetActiveCartByUserIdAsync(userId, cancellationToken);
        if (cart is null)
        {
            return Result.Failure<CartDto>(CartErrors.CartNotFound);
        }

        var cartItem = await _cartRepository.GetCartItemAsync(cart.Cartid, request.VariantId, cancellationToken);
        if (cartItem is null)
        {
            return Result.Failure<CartDto>(CartErrors.ItemNotFound);
        }

        _cartRepository.RemoveCartItem(cartItem);
        cart.Updatedat = DateTime.UtcNow;
        _cartRepository.UpdateCart(cart);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedCart = await _cartRepository.GetCartWithDetailsAsync(userId, cancellationToken);
        return Result.Success(CartMapper.ToDto(updatedCart ?? cart));
    }
}
