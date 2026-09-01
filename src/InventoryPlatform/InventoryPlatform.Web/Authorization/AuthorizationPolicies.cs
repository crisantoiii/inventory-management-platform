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

    public static class PurchaseOrder
    {
        public const string View = "PurchaseOrder.View";
        public const string Create = "PurchaseOrder.Create";
        public const string Submit = "PurchaseOrder.Submit";
        public const string Approve = "PurchaseOrder.Approve";
        public const string Receive = "PurchaseOrder.Receive";

        public const string ViewPolicy = "Capability:" + View;
        public const string CreatePolicy = "Capability:" + Create;
        public const string SubmitPolicy = "Capability:" + Submit;
        public const string ApprovePolicy = "Capability:" + Approve;
        public const string ReceivePolicy = "Capability:" + Receive;
    }
}
