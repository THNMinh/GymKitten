using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Order.Commands.Checkout;

public sealed record CheckoutCommand(
    List<CheckoutItemDto> Items,
    string ShippingAddress,
    string PaymentMethod,
    string? CustomerNote = null) : ICommand<Result<CheckoutCommandResponse>>;
