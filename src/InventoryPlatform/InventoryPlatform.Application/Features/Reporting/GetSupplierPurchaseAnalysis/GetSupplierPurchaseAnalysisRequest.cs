using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Shared.Paging;

namespace InventoryPlatform.Application.Features.Reporting.GetSupplierPurchaseAnalysis;

public sealed record GetSupplierPurchaseAnalysisRequest : PagedRequest
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    public PurchaseOrderStatus? PurchaseOrderStatus { get; init; }
}
