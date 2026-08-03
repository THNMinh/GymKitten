using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Catalog.Categories.Queries.GetAllCategories;

public sealed class GetAllCategoriesQueryHandler
    : IQueryHandler<GetAllCategoriesQuery, Result<List<CategoryDto>>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetAllCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<List<CategoryDto>>> Handle(
        GetAllCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);

        var dtos = categories.Select(c => new CategoryDto(
            c.Categoryid,
            c.Parentcategoryid,
            c.Name,
            c.Slug,
            c.Description,
            c.Displayorder,
            c.Createdat)).ToList();

        return Result.Success(dtos);
    }
}
