using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Shared.Paging;

namespace InventoryPlatform.Application.Features.Purchasing.GetPurchaseOrders;

public sealed record GetPurchaseOrdersRequest : PagedRequest
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    public PurchaseOrderStatus? PurchaseOrderStatus { get; init; }
}
