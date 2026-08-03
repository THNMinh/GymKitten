using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Catalog.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(
    Guid CategoryId,
    Guid? ParentCategoryId,
    string Name,
    string Slug,
    string? Description,
    int DisplayOrder) : ICommand<Result<UpdateCategoryResponse>>;
