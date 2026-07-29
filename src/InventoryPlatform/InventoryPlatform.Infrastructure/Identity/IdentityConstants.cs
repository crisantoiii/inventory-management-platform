namespace InventoryPlatform.Infrastructure.Identity;

public static class IdentityConstants
{
    public static class Roles
    {
        public const string Administrator = nameof(Administrator);
        public const string InventoryManager = nameof(InventoryManager);
        public const string Viewer = nameof(Viewer);
    }

    public static class DefaultAdmin
    {
        public const string UserName = "admin";
        public const string Email = "admin@inventory.local";
        public const string Password = "Admin@123";
    }
}