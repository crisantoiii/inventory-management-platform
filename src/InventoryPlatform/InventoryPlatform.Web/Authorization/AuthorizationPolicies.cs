namespace InventoryPlatform.Web.Authorization;

public static class AuthorizationPolicies
{
    public const string Administrator = nameof(Administrator);

    public const string InventoryManagement = nameof(InventoryManagement);

    public const string ViewInventory = nameof(ViewInventory);

    public const string AdministrationAccess = "Administration.Access";

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

    public static class ViewInventoryCapabilities
    {
        public const string DashboardView = "Dashboard.View";
        public const string ProductView = "Product.View";
        public const string CategoryView = "Category.View";
        public const string UnitView = "Unit.View";
        public const string CustomerView = "Customer.View";
        public const string SupplierView = "Supplier.View";
        public const string InventoryTransactionView = "InventoryTransaction.View";
    }

    public static class InventoryManagementCapabilities
    {
        public const string ProductCreate = "Product.Create";
        public const string ProductEdit = "Product.Edit";
        public const string CategoryCreate = "Category.Create";
        public const string SupplierCreate = "Supplier.Create";
        public const string SupplierEdit = "Supplier.Edit";
        public const string CustomerCreate = "Customer.Create";
        public const string CustomerEdit = "Customer.Edit";
        public const string UnitEdit = "Unit.Edit";
        public const string InventoryTransactionCreate = "InventoryTransaction.Create";
    }
}
