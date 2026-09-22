using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Features.Cart.DTOs;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Cart.Commands.AddToCart;

public sealed record AddToCartCommand(Guid VariantId, int Quantity = 1)
    : ICommand<Result<CartDto>>;
