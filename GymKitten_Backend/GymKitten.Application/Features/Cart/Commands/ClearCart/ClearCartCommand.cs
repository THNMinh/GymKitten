using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Cart.Commands.ClearCart;

public sealed record ClearCartCommand : ICommand<Result>;
