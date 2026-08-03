using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Catalog.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand(
    Guid? ParentCategoryId,
    string Name,
    string Slug,
    string? Description,
    int DisplayOrder) : ICommand<Result<CreateCategoryResponse>>;
