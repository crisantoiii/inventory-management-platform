using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Categories;

public static class CategoryErrors
{
    public static readonly Error NotFound =
        new(
            "Category.NotFound",
            "Category not found.");

    public static readonly Error DuplicateName =
        new(
            "Category.DuplicateName",
            "A category with the same Name already exists.");
}