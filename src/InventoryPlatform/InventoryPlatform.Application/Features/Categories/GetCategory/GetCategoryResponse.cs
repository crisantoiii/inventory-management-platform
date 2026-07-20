namespace InventoryPlatform.Application.Features.Categories.GetCategory;

public sealed record GetCategoryResponse(
    int Id,
    string Name,
    string? Description,
    bool IsActive);