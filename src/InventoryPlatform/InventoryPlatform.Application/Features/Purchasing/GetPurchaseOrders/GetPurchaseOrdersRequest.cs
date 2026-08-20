using InventoryPlatform.Domain.Enums;

namespace InventoryPlatform.Application.Features.Purchasing.GetPurchaseOrders;

public sealed record GetPurchaseOrdersRequest
{
    public string Search { get; init; } = string.Empty;

    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    public PurchaseOrderStatus? Status { get; init; }
}
