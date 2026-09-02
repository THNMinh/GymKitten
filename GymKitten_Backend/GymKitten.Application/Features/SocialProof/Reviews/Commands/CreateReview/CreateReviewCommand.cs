using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.SocialProof.Reviews.Commands.CreateReview;

public sealed record CreateReviewCommand(
    Guid ProductId,
    Guid OrderId,
    int Rating,
    string? Comment,
    string? FitFeedback) : ICommand<Result<CreateReviewResponse>>;

public sealed record CreateReviewResponse(Guid ReviewId);
