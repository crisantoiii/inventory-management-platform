using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Shared.Paging;

namespace InventoryPlatform.Application.Features.Reporting.GetPurchaseHistory;

public sealed record GetPurchaseHistoryRequest : PagedRequest
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    public PurchaseOrderStatus? PurchaseOrderStatus { get; init; }
}