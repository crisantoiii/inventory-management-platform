using InventoryPlatform.Infrastructure.Identity;

namespace InventoryPlatform.Web.Tests.Authentication;

public static class TestAuthenticationDefaults
{
    public const string Scheme = "SeededTestUser";

    public const string UserHeader = "X-Test-User";
}

public static class TestUserSelectors
{
    public const string Administrator = IdentityConstants.DefaultAdmin.Email;

    public const string InventoryManager = IdentityConstants.DefaultManager.Email;

    public const string Viewer = IdentityConstants.DefaultViewer.Email;

    public static bool IsSupported(string selector)
    {
        return selector.Equals(Administrator, StringComparison.OrdinalIgnoreCase) ||
               selector.Equals(InventoryManager, StringComparison.OrdinalIgnoreCase) ||
               selector.Equals(Viewer, StringComparison.OrdinalIgnoreCase);
    }
}
