using Microsoft.AspNetCore.Authorization;

namespace InventoryPlatform.Web.Authorization;

public sealed class CapabilityRequirement : IAuthorizationRequirement
{
    public CapabilityRequirement(string capabilityName)
    {
        if (string.IsNullOrWhiteSpace(capabilityName))
        {
            throw new ArgumentException(
                "Capability name is required.",
                nameof(capabilityName));
        }

        CapabilityName = capabilityName;
    }

    public string CapabilityName { get; }
}
