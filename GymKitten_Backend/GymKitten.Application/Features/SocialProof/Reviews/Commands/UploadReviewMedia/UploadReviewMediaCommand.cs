using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;
using Microsoft.AspNetCore.Http;

namespace GymKitten.Application.Features.SocialProof.Reviews.Commands.UploadReviewMedia;

public sealed record UploadReviewMediaCommand(
    Guid ReviewId,
    List<IFormFile> MediaFiles) : ICommand<Result<UploadReviewMediaResponse>>;

public sealed record UploadReviewMediaResponse(
    Guid ReviewId,
    List<ReviewMediaDto> MediaList);

public sealed record ReviewMediaDto(
    Guid MediaId,
    string MediaUrl,
    string MediaType);
