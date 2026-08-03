using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Catalog.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler
    : ICommandHandler<UpdateProductCommand, Result<UpdateProductResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UpdateProductResponse>> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get existing product via Repository
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure<UpdateProductResponse>(ProductErrors.NotFound);
        }

        // 2. Check slug uniqueness if changed via Repository
        if (product.Slug != request.Slug)
        {
            var slugExists = await _productRepository.ExistsBySlugExcludingIdAsync(request.Slug, request.ProductId, cancellationToken);

            if (slugExists)
            {
                return Result.Failure<UpdateProductResponse>(ProductErrors.SlugAlreadyExists);
            }
        }

        // 3. Update properties
        product.Categoryid = request.CategoryId;
        product.Name = request.Name;
        product.Slug = request.Slug;
        product.Description = request.Description;
        product.Fittype = request.FitType;
        product.Gender = request.Gender;
        product.Isactive = request.IsActive;
        product.Updatedat = DateTime.UtcNow;

        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateProductResponse(
            product.Productid,
            product.Name,
            product.Slug,
            product.Isactive));
    }
}
