using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.SocialProof.Wishlists.Commands.ToggleWishlist;

public sealed record ToggleWishlistCommand(Guid ProductId)
    : ICommand<Result<ToggleWishlistResponse>>;
