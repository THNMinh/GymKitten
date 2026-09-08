using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Abstractions.Services;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Catalog.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler
    : ICommandHandler<UpdateProductCommand, Result<UpdateProductResponse>>
{
    private readonly IUserContext _userContext;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationHubService _notificationHubService;

    public UpdateProductCommandHandler(
        IUserContext userContext,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        INotificationHubService notificationHubService)
    {
        _userContext = userContext;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _notificationHubService = notificationHubService;
    }

    public async Task<Result<UpdateProductResponse>> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(_userContext.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<UpdateProductResponse>(UserErrors.Forbidden);
        }

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

        await _notificationHubService.BroadcastNotificationAsync(new
        {
            type = "PRODUCT_UPDATED",
            productId = product.Productid,
            isActive = product.Isactive
        }, cancellationToken);

        return Result.Success(new UpdateProductResponse(
            product.Productid,
            product.Name,
            product.Slug,
            product.Isactive));
    }
}
