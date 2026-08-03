using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Catalog.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid ProductId,
    Guid CategoryId,
    string Name,
    string Slug,
    string? Description,
    string? FitType,
    string Gender,
    bool IsActive) : ICommand<Result<UpdateProductResponse>>;
