using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Catalog.Categories.Commands.DeleteCategory;

public sealed class DeleteCategoryCommandHandler
    : ICommandHandler<DeleteCategoryCommand, Result>
{
    private readonly IUserContext _userContext;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(
        IUserContext userContext,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteCategoryCommand request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(_userContext.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure(UserErrors.Forbidden);
        }

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
        {
            return Result.Failure(CategoryErrors.NotFound);
        }

        var hasProducts = await _categoryRepository.HasProductsAsync(request.CategoryId, cancellationToken);
        if (hasProducts)
        {
            return Result.Failure(CategoryErrors.HasAssociatedProducts);
        }

        _categoryRepository.Remove(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
