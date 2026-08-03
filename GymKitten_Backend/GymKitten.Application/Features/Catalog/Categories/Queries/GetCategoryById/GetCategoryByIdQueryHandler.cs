using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Features.Catalog.Categories.Queries.GetAllCategories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Catalog.Categories.Queries.GetCategoryById;

public sealed class GetCategoryByIdQueryHandler
    : IQueryHandler<GetCategoryByIdQuery, Result<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<CategoryDto>> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
        {
            return Result.Failure<CategoryDto>(CategoryErrors.NotFound);
        }

        var dto = new CategoryDto(
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
