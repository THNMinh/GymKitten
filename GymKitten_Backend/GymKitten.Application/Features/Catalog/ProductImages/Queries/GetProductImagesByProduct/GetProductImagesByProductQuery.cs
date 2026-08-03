using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Features.Catalog.ProductImages.Commands.UploadProductImages;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Catalog.ProductImages.Queries.GetProductImagesByProduct;

public sealed record GetProductImagesByProductQuery(Guid ProductId)
    : IQuery<Result<List<ProductImageDto>>>;
