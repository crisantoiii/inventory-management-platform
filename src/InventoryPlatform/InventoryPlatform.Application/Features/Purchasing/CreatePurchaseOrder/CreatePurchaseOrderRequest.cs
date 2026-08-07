namespace InventoryPlatform.Application.Features.Purchasing.CreatePurchaseOrder;

public sealed record CreatePurchaseOrderRequest(
    int SupplierId,
    DateOnly ExpectedDeliveryDate,
    string? Remarks,
    IReadOnlyCollection<CreatePurchaseOrderItemRequest> Items);