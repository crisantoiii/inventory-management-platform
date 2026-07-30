namespace InventoryPlatform.Web.Authorization;

public static class AuthorizationPolicies
{
    public const string Administrator = nameof(Administrator);

    public const string InventoryManagement = nameof(InventoryManagement);

    public const string ViewInventory = nameof(ViewInventory);
}