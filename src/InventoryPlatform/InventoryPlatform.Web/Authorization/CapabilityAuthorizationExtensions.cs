using Microsoft.AspNetCore.Authorization;

namespace InventoryPlatform.Web.Authorization;

public static class CapabilityAuthorizationExtensions
{
    public static void AddCapabilityPolicy(
        this AuthorizationOptions options,
        string policyName,
        string capabilityName)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(policyName))
        {
            throw new ArgumentException(
                "Policy name is required.",
                nameof(policyName));
        }

        if (string.IsNullOrWhiteSpace(capabilityName))
        {
            throw new ArgumentException(
                "Capability name is required.",
                nameof(capabilityName));
        }

        options.AddPolicy(
            policyName,
            policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(
                    new CapabilityRequirement(capabilityName));
            });
    }
}
