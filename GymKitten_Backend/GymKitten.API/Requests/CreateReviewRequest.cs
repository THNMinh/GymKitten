namespace GymKitten.API.Requests;

public sealed record CreateReviewRequest(
    Guid ProductId,
    Guid OrderId,
    int Rating,
    string? Comment,
    string? FitFeedback);
