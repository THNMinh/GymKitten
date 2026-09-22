using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Features.Cart.DTOs;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Cart.Commands.UpdateCartItem;

public sealed class UpdateCartItemCommandHandler : ICommandHandler<UpdateCartItemCommand, Result<CartDto>>
{
    private readonly ICartRepository _cartRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCartItemCommandHandler(
        ICartRepository cartRepository,
        IInventoryRepository inventoryRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _inventoryRepository = inventoryRepository;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CartDto>> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
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

        if (request.Quantity <= 0)
        {
            _cartRepository.RemoveCartItem(cartItem);
        }
        else
        {
            var inventory = await _inventoryRepository.GetByVariantIdAsync(request.VariantId, cancellationToken);
            var availableStock = inventory != null ? Math.Max(0, inventory.Quantityonhand - inventory.Quantityreserved) : 0;
            if (availableStock < request.Quantity)
            {
                return Result.Failure<CartDto>(CartErrors.InsufficientStock);
            }

            cartItem.Quantity = request.Quantity;
            cartItem.Updatedat = DateTime.UtcNow;
            _cartRepository.UpdateCartItem(cartItem);
        }

        cart.Updatedat = DateTime.UtcNow;
        _cartRepository.UpdateCart(cart);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedCart = await _cartRepository.GetCartWithDetailsAsync(userId, cancellationToken);
        return Result.Success(CartMapper.ToDto(updatedCart ?? cart));
    }
}
