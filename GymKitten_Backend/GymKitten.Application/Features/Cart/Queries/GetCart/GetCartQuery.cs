using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Features.Cart.DTOs;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Cart.Queries.GetCart;

public sealed record GetCartQuery : IQuery<Result<CartDto>>;
