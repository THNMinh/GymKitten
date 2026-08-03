using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Entities;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Catalog.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandler
    : ICommandHandler<CreateCategoryCommand, Result<CreateCategoryResponse>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateCategoryResponse>> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var parentCategoryId = (request.ParentCategoryId.HasValue && request.ParentCategoryId.Value != Guid.Empty)
            ? request.ParentCategoryId
            : null;

        // 1. Check parent category existence if provided
        if (parentCategoryId.HasValue)
        {
            var parentExists = await _categoryRepository.ExistsByIdAsync(parentCategoryId.Value, cancellationToken);
            if (!parentExists)
            {
                return Result.Failure<CreateCategoryResponse>(CategoryErrors.ParentCategoryNotFound);
            }
        }

        // 2. Check slug uniqueness
        var slugExists = await _categoryRepository.ExistsBySlugAsync(request.Slug, cancellationToken);
        if (slugExists)
        {
            return Result.Failure<CreateCategoryResponse>(CategoryErrors.SlugAlreadyExists);
        }

        // 3. Create entity
        var now = DateTime.UtcNow;
        var category = new Category
        {
            Categoryid = Guid.NewGuid(),
            Parentcategoryid = parentCategoryId,
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            Displayorder = request.DisplayOrder,
            Createdat = now,
            Updatedat = now
        };

        await _categoryRepository.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateCategoryResponse(category.Categoryid));
    }
}
