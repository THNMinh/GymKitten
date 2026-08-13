using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Catalog.ProductVariants.Commands.DeleteProductVariant;

public sealed record DeleteProductVariantCommand(Guid VariantId) : ICommand<Result>;
