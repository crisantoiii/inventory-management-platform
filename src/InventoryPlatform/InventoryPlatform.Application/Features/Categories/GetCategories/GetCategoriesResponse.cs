namespace InventoryPlatform.Application.Features.Categories.GetCategories;

public sealed record GetCategoriesResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public bool IsActive { get; init; }
}