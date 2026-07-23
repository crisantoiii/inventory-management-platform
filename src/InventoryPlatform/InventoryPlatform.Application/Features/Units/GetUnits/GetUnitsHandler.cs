using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Results;
namespace InventoryPlatform.Application.Features.Units.GetUnits;

public sealed class GetUnitsHandler
{
    private readonly IUnitRepository _unitRepository;

    public GetUnitsHandler(
        IUnitRepository unitRepository)
    {
        _unitRepository = unitRepository;
    }

    public async Task<Result<PagedResult<GetUnitsResponse>>> HandleAsync(
        GetUnitsRequest request,
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

        var units = await _unitRepository.GetPagedAsync(
            query,
            cancellationToken);

        var response = new PagedResult<GetUnitsResponse>
        {
            Items = units.Items
                .Select(unit => new GetUnitsResponse
                {
                    Id = unit.Id,
                    Code = unit.Code,
                    Name = unit.Name,
                    Symbol = unit.Symbol,
                    IsActive = unit.IsActive
                }
                    )
                .ToList(),

            Page = units.Page,
            PageSize = units.PageSize,
            TotalCount = units.TotalCount
        };

        return Result<PagedResult<GetUnitsResponse>>.Success(response);
    }
}