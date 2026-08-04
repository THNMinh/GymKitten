using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Features.Catalog.Categories.Queries.GetAllCategories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Catalog.Categories.Queries.GetCategoryById;

public sealed class GetCategoryByIdQueryHandler
    : IQueryHandler<GetCategoryByIdQuery, Result<CategoryItemDto>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<CategoryItemDto>> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
        {
            return Result.Failure<CategoryItemDto>(CategoryErrors.NotFound);
        }

        var dto = new CategoryItemDto(
            category.Categoryid,
            category.Parentcategoryid,
            category.Name,
            category.Slug,
            category.Description,
            category.Displayorder,
            category.Createdat);

        return Result.Success(dto);
    }
}
