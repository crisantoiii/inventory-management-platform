namespace InventoryPlatform.Application.Features.Categories.ActivateCategory;

public sealed record ActivateCategoryResponse(
    int Id,
    string Name,
    bool IsActive);