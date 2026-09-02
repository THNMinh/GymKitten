using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Abstractions.Storage;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Admin.Reviews.Commands.DeleteReview;

public sealed class DeleteReviewCommandHandler
    : ICommandHandler<DeleteReviewCommand, Result>
{
    private readonly IUserContext _userContext;
    private readonly IReviewRepository _reviewRepository;
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteReviewCommandHandler(
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

    public async Task<Result> Handle(
        DeleteReviewCommand request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(_userContext.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure(UserErrors.Forbidden);
        }

        var review = await _reviewRepository.GetByIdWithMediaAsync(request.ReviewId, cancellationToken);
        if (review is null)
        {
            return Result.Failure(ReviewErrors.NotFound);
        }

        // Collect object names for physical cleanup from MinIO
        var objectNamesToDelete = review.Reviewmedia
            .Select(m => ExtractObjectNameFromUrl(m.Mediaurl))
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToList();

        // Remove entity from DB (cascade removes Reviewmedia)
        _reviewRepository.Remove(review);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Physical cleanup from MinIO S3
        foreach (var objectName in objectNamesToDelete)
        {
            await _storageService.DeleteAsync(objectName, cancellationToken);
        }

        return Result.Success();
    }

    private static string ExtractObjectNameFromUrl(string mediaUrl)
    {
        if (string.IsNullOrWhiteSpace(mediaUrl)) return string.Empty;

        const string bucketMarker = "/gymkitten-media/";
        var index = mediaUrl.IndexOf(bucketMarker, StringComparison.OrdinalIgnoreCase);
        if (index != -1)
        {
            return mediaUrl[(index + bucketMarker.Length)..];
        }

        return mediaUrl;
    }
}
