using GymKitten.Domain.Common;

namespace GymKitten.Domain.Errors;

public static class CategoryErrors
{
    public static readonly Error NotFound = new(
        "Category.NotFound",
        "The specified category was not found.");

    public static readonly Error SlugAlreadyExists = new(
        "Category.SlugAlreadyExists",
        "A category with this slug already exists.");

    public static readonly Error ParentCategoryNotFound = new(
        "Category.ParentCategoryNotFound",
        "The specified parent category was not found.");

    public static readonly Error HasAssociatedProducts = new(
        "Category.HasAssociatedProducts",
        "Cannot delete category because it has associated products.");
}
