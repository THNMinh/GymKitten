using GymKitten.Domain.Common;

namespace GymKitten.Domain.Errors;

public static class ReviewErrors
{
    public static readonly Error NotFound = new(
        "Review.NotFound",
        "The specified review was not found.");

    public static readonly Error InvalidRating = new(
        "Review.InvalidRating",
        "Rating must be an integer between 1 and 5.");

    public static readonly Error InvalidFitFeedback = new(
        "Review.InvalidFitFeedback",
        "Fit feedback must be one of: TrueToSize, RunsSmall, RunsLarge.");

    public static readonly Error OrderNotFound = new(
        "Review.OrderNotFound",
        "The specified order was not found or does not belong to the user.");

    public static readonly Error ProductNotInOrder = new(
        "Review.ProductNotInOrder",
        "The specified product was not purchased in this order.");

    public static readonly Error AlreadyReviewed = new(
        "Review.AlreadyReviewed",
        "You have already reviewed this product for this order.");

    public static readonly Error UploadFailed = new(
        "Review.UploadFailed",
        "Failed to upload media for the review.");
}
