using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Features.Catalog.ProductImages.Commands.UploadProductImages;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Catalog.ProductImages.Queries.GetProductImagesByProduct;

public sealed class GetProductImagesByProductQueryHandler
    : IQueryHandler<GetProductImagesByProductQuery, Result<List<ProductImageDto>>>
{
    private readonly IProductImageRepository _productImageRepository;

    public GetProductImagesByProductQueryHandler(IProductImageRepository productImageRepository)
    {
        _productImageRepository = productImageRepository;
    }

    public async Task<Result<List<ProductImageDto>>> Handle(
        GetProductImagesByProductQuery request,
        CancellationToken cancellationToken)
    {
        var productExists = await _productImageRepository.ProductExistsAsync(request.ProductId, cancellationToken);
        if (!productExists)
        {
            return Result.Failure<List<ProductImageDto>>(ProductErrors.NotFound);
        }

        var images = await _productImageRepository.GetByProductIdAsync(request.ProductId, cancellationToken);

        var dtos = images.Select(img => new ProductImageDto(
            img.Imageid,
            img.Productid,
            img.Variantid,
            img.Imageurl,
            img.Displayorder,
            img.Isprimary,
            img.Variant?.Colorname,
            img.Variant?.Colorhex)).ToList();

        return Result.Success(dtos);
    }
}
