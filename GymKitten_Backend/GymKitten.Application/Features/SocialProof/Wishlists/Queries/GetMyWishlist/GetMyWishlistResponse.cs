namespace GymKitten.Application.Features.SocialProof.Wishlists.Queries.GetMyWishlist;

public sealed record GetMyWishlistResponse(
    List<WishlistItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
