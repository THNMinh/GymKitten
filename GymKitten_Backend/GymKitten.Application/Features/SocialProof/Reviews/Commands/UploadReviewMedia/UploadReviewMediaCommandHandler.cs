using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Abstractions.Storage;
using GymKitten.Domain.Common;
using GymKitten.Domain.Entities;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.SocialProof.Reviews.Commands.UploadReviewMedia;

public sealed class UploadReviewMediaCommandHandler
    : ICommandHandler<UploadReviewMediaCommand, Result<UploadReviewMediaResponse>>
{
    private readonly IUserContext _userContext;
    private readonly IReviewRepository _reviewRepository;
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;

    public UploadReviewMediaCommandHandler(
        IUserContext userContext,
        IReviewRepository reviewRepository,
        IStorageService storageService,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _reviewRepository = reviewRepository;
        _storageService = storageService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UploadReviewMediaResponse>> Handle(
        UploadReviewMediaCommand request,
        CancellationToken cancellationToken)
    {
        if (!_userContext.UserId.HasValue)
        {
            return Result.Failure<UploadReviewMediaResponse>(UserErrors.Unauthorized);
        }

        var review = await _reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken);
        if (review is null)
        {
            return Result.Failure<UploadReviewMediaResponse>(ReviewErrors.NotFound);
        }

        // Security check: Only review owner or admin can upload media
        if (review.Userid != _userContext.UserId.Value &&
            !string.Equals(_userContext.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<UploadReviewMediaResponse>(UserErrors.Forbidden);
        }

        if (request.MediaFiles is null || request.MediaFiles.Count == 0)
        {
            return Result.Success(new UploadReviewMediaResponse(request.ReviewId, new List<ReviewMediaDto>()));
        }

        // Upload files to MinIO S3
        var uploadResults = await _storageService.UploadMultipleAsync(request.MediaFiles, cancellationToken);

        // Safe Rollback if any upload failed
        if (uploadResults.Any(r => !r.Success))
        {
            foreach (var succeededUpload in uploadResults.Where(r => r.Success))
            {
                var objectName = ExtractObjectNameFromUrl(succeededUpload.PublicUrl);
                await _storageService.DeleteAsync(objectName, cancellationToken);
            }

            return Result.Failure<UploadReviewMediaResponse>(ReviewErrors.UploadFailed);
        }

        var now = DateTime.UtcNow;
        var mediaEntities = new List<Reviewmedia>();
        var dtos = new List<ReviewMediaDto>();

        for (int i = 0; i < uploadResults.Count; i++)
        {
            var upload = uploadResults[i];
            var formFile = request.MediaFiles[i];
            var isVideo = formFile.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase) ||
                          formFile.FileName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
                          formFile.FileName.EndsWith(".mov", StringComparison.OrdinalIgnoreCase) ||
                          formFile.FileName.EndsWith(".webm", StringComparison.OrdinalIgnoreCase);

            var mediaType = isVideo ? "video" : "image";

            var entity = new Reviewmedia
            {
                Mediaid = Guid.NewGuid(),
                Reviewid = request.ReviewId,
                Mediaurl = upload.PublicUrl,
                Mediatype = mediaType,
                Createdat = now,
                Updatedat = now
            };

            mediaEntities.Add(entity);
            dtos.Add(new ReviewMediaDto(entity.Mediaid, entity.Mediaurl, entity.Mediatype));
        }

        await _reviewRepository.AddMediaAsync(mediaEntities, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UploadReviewMediaResponse(request.ReviewId, dtos));
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
