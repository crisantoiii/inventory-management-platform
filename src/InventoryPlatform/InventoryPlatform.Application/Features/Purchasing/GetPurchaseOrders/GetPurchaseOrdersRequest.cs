namespace InventoryPlatform.Application.Features.Purchasing.GetPurchaseOrders;

public sealed record GetPurchaseOrdersRequest
{
    public string Search { get; init; } = string.Empty;
}
