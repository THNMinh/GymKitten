using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Features.Catalog.Categories.Queries.GetAllCategories;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Catalog.Categories.Queries.GetCategoryById;

public sealed record GetCategoryByIdQuery(Guid CategoryId) : IQuery<Result<CategoryItemDto>>;
