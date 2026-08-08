using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Purchasing;

public static class PurchaseOrderErrors
{
    public static readonly Error NotFound =
        new(
            "PurchaseOrder.NotFound",
            "Purchase order not found.");

    public static readonly Error SupplierNotFound =
        new(
            "PurchaseOrder.SupplierNotFound",
            "Supplier not found.");

    public static readonly Error SupplierInactive =
        new(
            "PurchaseOrder.SupplierInactive",
            "The selected supplier is inactive.");

    public static Error ProductNotFound(int productId) =>
        new(
            "PurchaseOrder.ProductNotFound",
            $"Product with ID '{productId}' was not found.");

    public static Error ProductInactive(int productId) =>
        new(
            "PurchaseOrder.ProductInactive",
            $"Product with ID '{productId}' is inactive.");
}