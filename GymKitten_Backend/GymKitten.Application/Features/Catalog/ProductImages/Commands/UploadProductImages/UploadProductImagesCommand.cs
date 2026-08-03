using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;
using Microsoft.AspNetCore.Http;

namespace GymKitten.Application.Features.Catalog.ProductImages.Commands.UploadProductImages;

public sealed record UploadProductImagesCommand(
    Guid ProductId,
    Guid? VariantId,
    List<IFormFile> Photos) : ICommand<Result<UploadProductImagesResponse>>;
