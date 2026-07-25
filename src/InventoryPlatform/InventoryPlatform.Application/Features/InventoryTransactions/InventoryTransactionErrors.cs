using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.InventoryTransactions;

public static class InventoryTransactionErrors
{
    public static readonly Error NotFound =
        new(
            "InventoryTransaction.NotFound",
            "Inventory Transaction not found.");

    public static readonly Error ProductNotFound =
        new(
            "InventoryTransaction.ProductNotFound",
            "The specified product was not found..");

    public static readonly Error InvalidTransactionType =
        new(
            "InventoryTransaction.Invalid",
            "Inventory Transaction is invalid.");

    public static readonly Error InsufficientStock =
        new(
            "InventoryTransaction.InsufficientStock",
            "Insufficient stock available..");


}