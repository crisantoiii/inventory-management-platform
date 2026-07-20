namespace InventoryPlatform.Application.Features.Categories.UpdateCategory;

public sealed record UpdateCategoryRequest(
    int Id,
    string Name,
    string? Description);