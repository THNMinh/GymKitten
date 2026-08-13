using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Catalog.Products.Queries.GetProductBySlug;

public sealed record GetProductBySlugQuery(string Slug) : IQuery<Result<GetProductBySlugResponse>>;
