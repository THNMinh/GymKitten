using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Abstractions.Storage;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Catalog.Products.Commands.DeleteProduct;

public sealed class DeleteProductCommandHandler
    : ICommandHandler<DeleteProductCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductCommandHandler(
        IProductRepository productRepository,
        IStorageService storageService,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _storageService = storageService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get product with images via Repository
        var product = await _productRepository.GetProductWithImagesAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound);
        }

        // 2. Collect all object names from Imageurl for physical cleanup
        var objectNamesToDelete = product.Productimages
            .Select(img => ExtractObjectNameFromUrl(img.Imageurl))
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToList();

        // 3. Remove product entity from DB (cascade removes images from DB)
        _productRepository.Remove(product);

        // 4. Save DB changes FIRST
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 5. Physical MinIO cleanup AFTER DB transaction succeeds
        foreach (var objectName in objectNamesToDelete)
        {
            await _storageService.DeleteAsync(objectName, cancellationToken);
        }

        return Result.Success();
    }

    private static string ExtractObjectNameFromUrl(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl)) return string.Empty;

        const string bucketMarker = "/gymkitten-media/";
        var index = imageUrl.IndexOf(bucketMarker, StringComparison.OrdinalIgnoreCase);
        if (index != -1)
        {
            return imageUrl[(index + bucketMarker.Length)..];
        }

        return imageUrl;
    }
}
