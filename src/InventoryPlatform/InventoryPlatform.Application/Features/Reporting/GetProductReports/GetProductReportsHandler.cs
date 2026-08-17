using InventoryPlatform.Application.DTOs.Reporting;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Reporting.GetProductReports;

public sealed class GetProductReportsHandler
{
    private readonly IProductReportsRepository _repository;

    public GetProductReportsHandler(
        IProductReportsRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<ProductReportDto>>> HandleAsync(
        GetProductReportsRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _repository.GetProductReportsAsync(
            request.Query,
            cancellationToken);

        return Result<PagedResult<ProductReportDto>>.Success(result);
    }
}
