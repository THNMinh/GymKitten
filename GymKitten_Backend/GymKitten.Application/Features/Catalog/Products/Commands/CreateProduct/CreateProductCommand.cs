using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Catalog.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(
    Guid CategoryId,
    string Name,
    string Slug,
    string? Description,
    string? FitType,
    string Gender) : ICommand<Result<CreateProductResponse>>;
