using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Catalog.Categories.Queries.GetAllCategories;

public sealed class GetAllCategoriesQueryHandler
    : IQueryHandler<GetAllCategoriesQuery, Result<GetAllCategoriesResponse>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetAllCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<GetAllCategoriesResponse>> Handle(
        GetAllCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var (categories, total) = await _categoryRepository.SearchCategoriesAsync(
            request.SearchName,
            request.ParentCategoryId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var items = categories.Select(c => new CategoryItemDto(
            c.Categoryid,
            c.Parentcategoryid,
            c.Name,
            c.Slug,
            c.Description,
            c.Displayorder,
            c.Createdat)).ToList();

        var totalPages = (int)Math.Ceiling(total / (double)request.PageSize);

        var response = new GetAllCategoriesResponse(
            items,
            total,
            request.Page,
            request.PageSize,
            totalPages);

        return Result.Success(response);
    }
}
