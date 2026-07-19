namespace InventoryPlatform.Shared.Paging;

public abstract record PagedRequest
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 100;

    private int _page = 1;
    private int _pageSize = DefaultPageSize;

    public int Page
    {
        get => _page;
        init => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        init
        {
            if (value < 1)
                _pageSize = DefaultPageSize;
            else if (value > MaxPageSize)
                _pageSize = MaxPageSize;
            else
                _pageSize = value;
        }
    }

    public string? SortBy { get; init; }

    public bool Descending { get; init; }
}