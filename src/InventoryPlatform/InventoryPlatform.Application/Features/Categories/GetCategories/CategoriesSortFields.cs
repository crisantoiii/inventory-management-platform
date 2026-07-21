using InventoryPlatform.Domain.Entities;

namespace InventoryPlatform.Application.Features.Categories.GetCategories;

public static class CategoriesSortFields
{
    public const string Name = nameof(Category.Name);

    public const string IsActive = nameof(Category.IsActive);
}