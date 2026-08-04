namespace GymKitten.Application.Features.SocialProof.Wishlists.Commands.ToggleWishlist;

public sealed record ToggleWishlistResponse(
    bool IsAdded,
    string Message);
