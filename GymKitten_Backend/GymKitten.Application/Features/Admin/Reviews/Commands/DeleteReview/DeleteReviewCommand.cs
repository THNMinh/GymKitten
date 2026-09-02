using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Admin.Reviews.Commands.DeleteReview;

public sealed record DeleteReviewCommand(Guid ReviewId) : ICommand<Result>;
