using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Features.Cart.DTOs;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Cart.Commands.UpdateCartItem;

public sealed record UpdateCartItemCommand(Guid VariantId, int Quantity)
    : ICommand<Result<CartDto>>;
