using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Catalog.ProductImages.Commands.DeleteProductImage;

public sealed record DeleteProductImageCommand(Guid ImageId) : ICommand<Result>;
