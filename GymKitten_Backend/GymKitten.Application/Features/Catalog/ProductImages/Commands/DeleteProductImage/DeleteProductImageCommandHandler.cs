using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Abstractions.Storage;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Catalog.ProductImages.Commands.DeleteProductImage;

public sealed class DeleteProductImageCommandHandler
    : ICommandHandler<DeleteProductImageCommand, Result>
{
    private readonly IProductImageRepository _productImageRepository;
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductImageCommandHandler(
        IProductImageRepository productImageRepository,
        IStorageService storageService,
        IUnitOfWork unitOfWork)
    {
        _productImageRepository = productImageRepository;
        _storageService = storageService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteProductImageCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get image by ID via Repository
        var photo = await _productImageRepository.GetByIdAsync(request.ImageId, cancellationToken);
        if (photo is null)
        {
            return Result.Failure(ProductErrors.ImageNotFound);
        }

        // 2. Derive object name for physical deletion
        var objectName = ExtractObjectNameFromUrl(photo.Imageurl);

        // 3. Remove entity from DB
        _productImageRepository.Remove(photo);

        // 4. Save DB changes FIRST
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 5. Physical MinIO deletion AFTER DB transaction succeeds
        if (!string.IsNullOrWhiteSpace(objectName))
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
