using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Catalog.SizeGuides.Queries.GetProductSizeGuide;

public sealed class GetProductSizeGuideQueryHandler
    : IQueryHandler<GetProductSizeGuideQuery, Result<GetProductSizeGuideResponse>>
{
    private readonly ISizeGuideRepository _sizeGuideRepository;
    private readonly IProductRepository _productRepository;

    public GetProductSizeGuideQueryHandler(
        ISizeGuideRepository sizeGuideRepository,
        IProductRepository productRepository)
    {
        _sizeGuideRepository = sizeGuideRepository;
        _productRepository = productRepository;
    }

    public async Task<Result<GetProductSizeGuideResponse>> Handle(
        GetProductSizeGuideQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure<GetProductSizeGuideResponse>(ProductErrors.NotFound);
        }

        var guides = await _sizeGuideRepository.GetByProductIdAsync(request.ProductId, cancellationToken);

        var dtos = guides.Select(g => new SizeGuideDto(
            g.Guideid,
            g.Size,
            g.Chestcm,
            g.Waistcm,
            g.Hipscm,
            g.Heightrangecm
        )).ToList();

        return Result.Success(new GetProductSizeGuideResponse(request.ProductId, dtos));
    }
}
