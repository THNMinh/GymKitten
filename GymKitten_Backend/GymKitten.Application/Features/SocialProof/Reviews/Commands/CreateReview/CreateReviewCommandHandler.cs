using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Entities;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.SocialProof.Reviews.Commands.CreateReview;

public sealed class CreateReviewCommandHandler
    : ICommandHandler<CreateReviewCommand, Result<CreateReviewResponse>>
{
    private readonly IUserContext _userContext;
    private readonly IReviewRepository _reviewRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    private static readonly HashSet<string> AllowedFitFeedbacks = new(StringComparer.OrdinalIgnoreCase)
    {
        "TrueToSize",
        "RunsSmall",
        "RunsLarge"
    };

    public CreateReviewCommandHandler(
        IUserContext userContext,
        IReviewRepository reviewRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _reviewRepository = reviewRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateReviewResponse>> Handle(
        CreateReviewCommand request,
        CancellationToken cancellationToken)
    {
        if (!_userContext.UserId.HasValue)
        {
            return Result.Failure<CreateReviewResponse>(UserErrors.Unauthorized);
        }

        // 1. Validate Rating
        if (request.Rating < 1 || request.Rating > 5)
        {
            return Result.Failure<CreateReviewResponse>(ReviewErrors.InvalidRating);
        }

        // 2. Validate FitFeedback if provided
        string? normalizedFitFeedback = null;
        if (!string.IsNullOrWhiteSpace(request.FitFeedback))
        {
            var trimmed = request.FitFeedback.Trim();
            if (!AllowedFitFeedbacks.Contains(trimmed))
            {
                return Result.Failure<CreateReviewResponse>(ReviewErrors.InvalidFitFeedback);
            }
            normalizedFitFeedback = trimmed;
        }

        // 3. Verify Product exists (supports both ProductId and VariantId)
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        var resolvedProductId = request.ProductId;

        if (product is null)
        {
            var variant = await _productRepository.GetVariantByIdAsync(request.ProductId, cancellationToken);
            if (variant != null)
            {
                resolvedProductId = variant.Productid;
                product = await _productRepository.GetByIdAsync(resolvedProductId, cancellationToken);
            }
        }

        if (product is null)
        {
            return Result.Failure<CreateReviewResponse>(ProductErrors.NotFound);
        }

        // 4. Verify User purchased this product in this order
        var purchased = await _reviewRepository.UserPurchasedProductAsync(_userContext.UserId.Value, resolvedProductId, request.OrderId, cancellationToken);
        if (!purchased)
        {
            return Result.Failure<CreateReviewResponse>(ReviewErrors.ProductNotInOrder);
        }

        // 5. Verify user hasn't already reviewed this product for this order
        var alreadyReviewed = await _reviewRepository.ExistsByOrderAndProductAsync(request.OrderId, resolvedProductId, cancellationToken);
        if (alreadyReviewed)
        {
            return Result.Failure<CreateReviewResponse>(ReviewErrors.AlreadyReviewed);
        }

        // 6. Create Review entity
        var now = DateTime.UtcNow;
        var review = new Productreview
        {
            Reviewid = Guid.NewGuid(),
            Productid = resolvedProductId,
            Userid = _userContext.UserId.Value,
            Orderid = request.OrderId,
            Rating = request.Rating,
            Comment = request.Comment?.Trim(),
            Fitfeedback = normalizedFitFeedback,
            Createdat = now,
            Updatedat = now
        };

        await _reviewRepository.AddAsync(review, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateReviewResponse(review.Reviewid));
    }
}
