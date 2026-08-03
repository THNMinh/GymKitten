using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Catalog.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandHandler
    : ICommandHandler<UpdateCategoryCommand, Result<UpdateCategoryResponse>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UpdateCategoryResponse>> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get existing category
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
        {
            return Result.Failure<UpdateCategoryResponse>(CategoryErrors.NotFound);
        }

        var parentCategoryId = (request.ParentCategoryId.HasValue && request.ParentCategoryId.Value != Guid.Empty)
            ? request.ParentCategoryId
            : null;

        // 2. Check parent category existence if provided
        if (parentCategoryId.HasValue)
        {
            var parentExists = await _categoryRepository.ExistsByIdAsync(parentCategoryId.Value, cancellationToken);
            if (!parentExists)
            {
                return Result.Failure<UpdateCategoryResponse>(CategoryErrors.ParentCategoryNotFound);
            }
        }

        // 3. Check slug uniqueness if changed
        if (category.Slug != request.Slug)
        {
            var slugExists = await _categoryRepository.ExistsBySlugExcludingIdAsync(request.Slug, request.CategoryId, cancellationToken);
            if (slugExists)
            {
                return Result.Failure<UpdateCategoryResponse>(CategoryErrors.SlugAlreadyExists);
            }
        }

        // 4. Update entity
        category.Parentcategoryid = parentCategoryId;
        category.Name = request.Name;
        category.Slug = request.Slug;
        category.Description = request.Description;
        category.Displayorder = request.DisplayOrder;
        category.Updatedat = DateTime.UtcNow;

        _categoryRepository.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateCategoryResponse(
            category.Categoryid,
            category.Name,
            category.Slug));
    }
}
