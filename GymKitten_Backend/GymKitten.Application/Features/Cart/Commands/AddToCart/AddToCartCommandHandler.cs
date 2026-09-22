using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Features.Cart.DTOs;
using GymKitten.Domain.Common;
using GymKitten.Domain.Entities;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Cart.Commands.AddToCart;

public sealed class AddToCartCommandHandler : ICommandHandler<AddToCartCommand, Result<CartDto>>
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public AddToCartCommandHandler(
        ICartRepository cartRepository,
        IProductVariantRepository productVariantRepository,
        IInventoryRepository inventoryRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _productVariantRepository = productVariantRepository;
        _inventoryRepository = inventoryRepository;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CartDto>> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        if (!_userContext.IsAuthenticated || !_userContext.UserId.HasValue)
        {
            return Result.Failure<CartDto>(CartErrors.Unauthorized);
        }

        if (request.Quantity <= 0)
        {
            return Result.Failure<CartDto>(CartErrors.InvalidQuantity);
        }

        // 1. Verify variant exists
        var variant = await _productVariantRepository.GetByIdAsync(request.VariantId, cancellationToken);
        if (variant is null)
        {
            return Result.Failure<CartDto>(CartErrors.VariantNotFound);
        }

        // 2. Verify product is active and not deleted
        if (variant.Product is null || !variant.Product.Isactive || variant.Product.Deletedat != null)
        {
            return Result.Failure<CartDto>(CartErrors.ProductInactive);
        }

        var userId = _userContext.UserId.Value;

        // 3. Get or create active cart
        var cart = await _cartRepository.GetActiveCartByUserIdAsync(userId, cancellationToken);
        if (cart is null)
        {
            cart = new Domain.Entities.Cart
            {
                Cartid = Guid.NewGuid(),
                Userid = userId,
                Createdat = DateTime.UtcNow,
                Updatedat = DateTime.UtcNow
            };
            await _cartRepository.AddCartAsync(cart, cancellationToken);
        }

        // 4. Check existing cart item
        var cartItem = await _cartRepository.GetCartItemAsync(cart.Cartid, request.VariantId, cancellationToken);
        var requestedQuantity = (cartItem?.Quantity ?? 0) + request.Quantity;

        // 5. Verify available inventory stock
        var inventory = await _inventoryRepository.GetByVariantIdAsync(request.VariantId, cancellationToken);
        var availableStock = inventory != null ? Math.Max(0, inventory.Quantityonhand - inventory.Quantityreserved) : 0;
        if (availableStock < requestedQuantity)
        {
            return Result.Failure<CartDto>(CartErrors.InsufficientStock);
        }

        // 6. Update or Add CartItem
        if (cartItem != null)
        {
            cartItem.Quantity = requestedQuantity;
            cartItem.Updatedat = DateTime.UtcNow;
            _cartRepository.UpdateCartItem(cartItem);
        }
        else
        {
            cartItem = new Cartitem
            {
                Cartitemid = Guid.NewGuid(),
                Cartid = cart.Cartid,
                Variantid = request.VariantId,
                Quantity = request.Quantity,
                Createdat = DateTime.UtcNow,
                Updatedat = DateTime.UtcNow
            };
            await _cartRepository.AddCartItemAsync(cartItem, cancellationToken);
        }

        cart.Updatedat = DateTime.UtcNow;
        _cartRepository.UpdateCart(cart);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Reload and return updated cart details
        var updatedCart = await _cartRepository.GetCartWithDetailsAsync(userId, cancellationToken);
        return Result.Success(CartMapper.ToDto(updatedCart ?? cart));
    }
}
