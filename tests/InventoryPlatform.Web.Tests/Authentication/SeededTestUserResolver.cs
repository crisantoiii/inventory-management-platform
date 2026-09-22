using InventoryPlatform.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace InventoryPlatform.Web.Tests.Authentication;

public sealed class SeededTestUserResolver
{
    private readonly UserManager<ApplicationUser> _userManager;

    public SeededTestUserResolver(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<SeededTestUserResolution> ResolveAsync(string selector)
    {
        if (!TestUserSelectors.IsSupported(selector))
        {
            return SeededTestUserResolution.Failed(
                "The test-user selector is unknown.");
        }

        var user = await _userManager.FindByEmailAsync(selector);

        return user is null
            ? SeededTestUserResolution.Failed(
                $"Expected startup-seeded test user '{selector}' was not found.")
            : SeededTestUserResolution.Succeeded(user);
    }
}

public sealed record SeededTestUserResolution(
    ApplicationUser? User,
    string? FailureMessage)
{
    public bool IsSuccess => User is not null;

    public static SeededTestUserResolution Succeeded(ApplicationUser user) =>
        new(user, null);

    public static SeededTestUserResolution Failed(string message) =>
        new(null, message);
}
