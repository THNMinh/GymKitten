using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Entities;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Catalog.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler
    : ICommandHandler<CreateProductCommand, Result<CreateProductResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateProductResponse>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Verify slug uniqueness via Repository
        var slugExists = await _productRepository.ExistsBySlugAsync(request.Slug, cancellationToken);

        if (slugExists)
        {
            return Result.Failure<CreateProductResponse>(ProductErrors.SlugAlreadyExists);
        }

        // 2. Create Product entity
        var now = DateTime.UtcNow;
        var product = new Product
        {
            Productid = Guid.NewGuid(),
            Categoryid = request.CategoryId,
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            Fittype = request.FitType,
            Gender = request.Gender,
            Isactive = true,
            Createdat = now,
            Updatedat = now
        };

        await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateProductResponse(product.Productid));
    }
}
