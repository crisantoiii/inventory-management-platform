using InventoryPlatform.Shared.Paging;

namespace InventoryPlatform.Application.Features.Reporting.GetProductReports;

public sealed record GetProductReportsRequest(
    PagedQuery Query);
