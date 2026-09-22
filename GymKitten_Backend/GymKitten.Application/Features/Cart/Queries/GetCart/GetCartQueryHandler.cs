using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Features.Cart.DTOs;
using GymKitten.Domain.Common;
using GymKitten.Domain.Entities;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Cart.Queries.GetCart;

public sealed class GetCartQueryHandler : IQueryHandler<GetCartQuery, Result<CartDto>>
{
    private readonly ICartRepository _cartRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public GetCartQueryHandler(
        ICartRepository cartRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CartDto>> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        if (!_userContext.IsAuthenticated || !_userContext.UserId.HasValue)
        {
            return Result.Failure<CartDto>(CartErrors.Unauthorized);
        }

        var userId = _userContext.UserId.Value;
        var cart = await _cartRepository.GetCartWithDetailsAsync(userId, cancellationToken);

        if (cart is null)
        {
            // Auto create cart for the user if not exists
            cart = new Domain.Entities.Cart
            {
                Cartid = Guid.NewGuid(),
                Userid = userId,
                Createdat = DateTime.UtcNow,
                Updatedat = DateTime.UtcNow
            };

            await _cartRepository.AddCartAsync(cart, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success(CartMapper.ToDto(cart));
    }
}
