using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Entities;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Catalog.ProductVariants.Commands.CreateProductVariant;

public sealed class CreateProductVariantCommandHandler
    : ICommandHandler<CreateProductVariantCommand, Result<CreateProductVariantResponse>>
{
    private readonly IUserContext _userContext;
    private readonly IProductRepository _productRepository;
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductVariantCommandHandler(
        IUserContext userContext,
        IProductRepository productRepository,
        IProductVariantRepository productVariantRepository,
        IInventoryRepository inventoryRepository,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _productRepository = productRepository;
        _productVariantRepository = productVariantRepository;
        _inventoryRepository = inventoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateProductVariantResponse>> Handle(
        CreateProductVariantCommand request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(_userContext.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<CreateProductVariantResponse>(UserErrors.Forbidden);
        }

        // 1. Verify parent product exists
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure<CreateProductVariantResponse>(ProductErrors.NotFound);
        }

        // 2. Check SKU uniqueness & Price limits
        if (request.Price <= 0 || request.Price > 1000000000m || (request.OriginalPrice.HasValue && request.OriginalPrice.Value > 1000000000m))
        {
            return Result.Failure<CreateProductVariantResponse>(ProductVariantErrors.InvalidPrice);
        }

        // 3. Prevent duplicate Size for the same Color on this Product
        var duplicateColorAndSize = await _productVariantRepository.ExistsByProductColorAndSizeAsync(
            request.ProductId,
            request.ColorName,
            request.Size,
            null,
            cancellationToken);
        if (duplicateColorAndSize)
        {
            return Result.Failure<CreateProductVariantResponse>(ProductVariantErrors.DuplicateColorAndSize);
        }

        // 4. Determine SKU (auto-fallback if blank)
        var finalSku = !string.IsNullOrWhiteSpace(request.Sku)
            ? request.Sku.Trim().ToUpperInvariant()
            : $"GK-{product.Slug.ToUpperInvariant()}-{request.ColorName.Trim().ToUpperInvariant()}-{request.Size.Trim().ToUpperInvariant()}";
        finalSku = System.Text.RegularExpressions.Regex.Replace(finalSku, @"[^a-zA-Z0-9_-]", "-");

        var skuExists = await _productVariantRepository.ExistsBySkuAsync(finalSku, cancellationToken);
        if (skuExists)
        {
            return Result.Failure<CreateProductVariantResponse>(ProductVariantErrors.SkuAlreadyExists);
        }

        // 3. Create entity
        var now = DateTime.UtcNow;
        var variant = new Productvariant
        {
            Variantid = Guid.NewGuid(),
            Productid = request.ProductId,
            Sku = finalSku,
            Colorname = request.ColorName.Trim(),
            Colorhex = request.ColorHex?.Trim(),
            Size = request.Size.Trim(),
            Price = request.Price,
            Originalprice = request.OriginalPrice,
            Weightgrams = request.WeightGrams,
            Createdat = now,
            Updatedat = now
        };

        await _productVariantRepository.AddAsync(variant, cancellationToken);

        // 4. Automatically initialize an empty inventory item
        var emptyInventory = new Inventoryitem
        {
            Inventoryid = Guid.NewGuid(),
            Variantid = variant.Variantid,
            Quantityonhand = 0,
            Quantityreserved = 0,
            Safetystock = 5,
            Rowversion = 1,
            Createdat = now,
            Updatedat = now
        };

        await _inventoryRepository.AddAsync(emptyInventory, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            var innerMsg = ex.InnerException?.Message ?? ex.Message;
            if (innerMsg.Contains("23505") || innerMsg.Contains("duplicate key", StringComparison.OrdinalIgnoreCase))
            {
                if (innerMsg.Contains("sku", StringComparison.OrdinalIgnoreCase))
                {
                    return Result.Failure<CreateProductVariantResponse>(ProductVariantErrors.SkuAlreadyExists);
                }
                return Result.Failure<CreateProductVariantResponse>(ProductVariantErrors.DuplicateColorAndSize);
            }
            throw;
        }

        return Result.Success(new CreateProductVariantResponse(variant.Variantid));
    }
}
