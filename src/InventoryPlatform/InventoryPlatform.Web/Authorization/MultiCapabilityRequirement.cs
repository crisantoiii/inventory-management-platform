using Microsoft.AspNetCore.Authorization;

namespace InventoryPlatform.Web.Authorization;

public sealed class MultiCapabilityRequirement : IAuthorizationRequirement
{
    public IReadOnlyList<string> CapabilityNames { get; }

    public MultiCapabilityRequirement(params string[] capabilityNames)
    {
        if (capabilityNames is null || capabilityNames.Length == 0)
        {
            throw new ArgumentException(
                "At least one capability name is required.",
                nameof(capabilityNames));
        }

        foreach (var name in capabilityNames)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Capability names must not be null or whitespace.",
                    nameof(capabilityNames));
            }
        }

        CapabilityNames = capabilityNames.ToArray();
    }
}
