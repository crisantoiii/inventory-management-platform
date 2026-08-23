using InventoryPlatform.Shared.Filtering;

namespace InventoryPlatform.Shared.Paging;

public sealed record PagedQuery
{
    public int PageNum { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public string? Search { get; init; }

    public string? SortBy { get; init; }

    public bool Descending { get; init; }

    public ProductStatusFilter Status { get; init; }
}