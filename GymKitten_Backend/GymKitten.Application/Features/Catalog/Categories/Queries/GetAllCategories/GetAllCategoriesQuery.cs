using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Catalog.Categories.Queries.GetAllCategories;

public record CategoryDto(
    Guid CategoryId,
    Guid? ParentCategoryId,
    string Name,
    string Slug,
    string? Description,
    int DisplayOrder,
    DateTime CreatedAt);

public sealed record GetAllCategoriesQuery : IQuery<Result<List<CategoryDto>>>;
