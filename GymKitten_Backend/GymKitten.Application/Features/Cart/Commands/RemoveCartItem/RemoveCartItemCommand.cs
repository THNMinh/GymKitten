using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Features.Cart.DTOs;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Cart.Commands.RemoveCartItem;

public sealed record RemoveCartItemCommand(Guid VariantId)
    : ICommand<Result<CartDto>>;
