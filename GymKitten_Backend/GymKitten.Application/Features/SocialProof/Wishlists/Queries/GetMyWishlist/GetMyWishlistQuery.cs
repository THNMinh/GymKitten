using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.SocialProof.Wishlists.Queries.GetMyWishlist;

public sealed record GetMyWishlistQuery(
    int Page = 1,
    int PageSize = 10) : IQuery<Result<GetMyWishlistResponse>>;
