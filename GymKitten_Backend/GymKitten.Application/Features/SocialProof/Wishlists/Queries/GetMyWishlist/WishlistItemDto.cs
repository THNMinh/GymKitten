namespace GymKitten.Application.Features.SocialProof.Wishlists.Queries.GetMyWishlist;

public sealed record WishlistItemDto(
    Guid ProductId,
    string Name,
    string Slug,
    decimal MinPrice,
    string? PrimaryImageUrl,
    DateTime AddedAt);
