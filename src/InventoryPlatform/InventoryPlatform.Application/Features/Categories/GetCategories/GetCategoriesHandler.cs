using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Results;
namespace InventoryPlatform.Application.Features.Categories.GetCategories;

public sealed class GetCategoriesHandler
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoriesHandler(
        ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<PagedResult<GetCategoriesResponse>>> HandleAsync(
        GetCategoriesRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = new PagedQuery
        {
            Search = request.Search,
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            Descending = request.Descending,
            Status = request.Status
        };

        var categorys = await _categoryRepository.GetPagedAsync(
            query,
            cancellationToken);

        var response = new PagedResult<GetCategoriesResponse>
        {
            Items = categorys.Items
                .Select(category => new GetCategoriesResponse
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    IsActive = category.IsActive
                }
                    )
                .ToList(),

            Page = categorys.Page,
            PageSize = categorys.PageSize,
            TotalCount = categorys.TotalCount
        };

        return Result<PagedResult<GetCategoriesResponse>>.Success(response);
    }
}