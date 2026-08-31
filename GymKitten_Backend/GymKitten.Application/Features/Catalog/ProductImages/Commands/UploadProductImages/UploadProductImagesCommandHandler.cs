using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Abstractions.Storage;
using GymKitten.Domain.Common;
using GymKitten.Domain.Entities;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Catalog.ProductImages.Commands.UploadProductImages;

public sealed class UploadProductImagesCommandHandler
    : ICommandHandler<UploadProductImagesCommand, Result<UploadProductImagesResponse>>
{
    private readonly IUserContext _userContext;
    private readonly IProductImageRepository _productImageRepository;
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;

    public UploadProductImagesCommandHandler(
        IUserContext userContext,
        IProductImageRepository productImageRepository,
        IStorageService storageService,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _productImageRepository = productImageRepository;
        _storageService = storageService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UploadProductImagesResponse>> Handle(
        UploadProductImagesCommand request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(_userContext.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<UploadProductImagesResponse>(UserErrors.Forbidden);
        }

        // 1. Verify product exists
        var productExists = await _productImageRepository.ProductExistsAsync(request.ProductId, cancellationToken);
        if (!productExists)
        {
            return Result.Failure<UploadProductImagesResponse>(ProductErrors.NotFound);
        }

        // 2. Upload files to MinIO S3
        var uploadResults = await _storageService.UploadMultipleAsync(request.Photos, cancellationToken);

        // 3. SAFE ROLLBACK: If any upload failed, rollback all succeeded uploads
        if (uploadResults.Any(r => !r.Success))
        {
            foreach (var succeededUpload in uploadResults.Where(r => r.Success))
            {
                var objectName = ExtractObjectNameFromUrl(succeededUpload.PublicUrl);
                await _storageService.DeleteAsync(objectName, cancellationToken);
            }

            return Result.Failure<UploadProductImagesResponse>(ProductErrors.UploadFailed);
        }

        // 4. Calculate starting display order
        var currentMaxOrder = await _productImageRepository.GetMaxDisplayOrderAsync(request.ProductId, cancellationToken);
        var isFirstBatch = currentMaxOrder == 0;

        // Normalize optional VariantId (convert Guid.Empty to null)
        var variantId = (request.VariantId.HasValue && request.VariantId.Value != Guid.Empty)
            ? request.VariantId
            : null;

        var now = DateTime.UtcNow;
        var entities = new List<Productimage>();
        var dtos = new List<ProductImageDto>();

        for (int i = 0; i < uploadResults.Count; i++)
        {
            var upload = uploadResults[i];
            var isPrimary = isFirstBatch && i == 0;
            var displayOrder = ++currentMaxOrder;

            var entity = new Productimage
            {
                Imageid = Guid.NewGuid(),
                Productid = request.ProductId,
                Variantid = variantId,
                Imageurl = upload.PublicUrl,
                Displayorder = displayOrder,
                Isprimary = isPrimary,
                Createdat = now,
                Updatedat = now
            };

            entities.Add(entity);

            dtos.Add(new ProductImageDto(
                entity.Imageid,
                entity.Productid,
                entity.Variantid,
                entity.Imageurl,
                entity.Displayorder,
                entity.Isprimary));
        }

        // 5. Persist to DB
        await _productImageRepository.AddPhotosAsync(entities, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UploadProductImagesResponse(request.ProductId, dtos));
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
