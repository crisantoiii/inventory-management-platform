namespace InventoryPlatform.Web.Authorization;

public static class AuthorizationPolicies
{
    public const string Administrator = nameof(Administrator);

    public const string InventoryManagement = nameof(InventoryManagement);

    public const string ViewInventory = nameof(ViewInventory);

    public static string ForCapability(string capabilityName)
    {
        if (string.IsNullOrWhiteSpace(capabilityName))
        {
            throw new ArgumentException(
                "Capability name is required.",
                nameof(capabilityName));
        }

        return $"Capability:{capabilityName}";
    }
}
