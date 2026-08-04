using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.SocialProof.Wishlists.Commands.RemoveWishlist;

public sealed record RemoveWishlistCommand(Guid ProductId) : ICommand<Result>;
