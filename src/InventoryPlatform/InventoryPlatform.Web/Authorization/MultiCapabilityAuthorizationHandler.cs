using System.Security.Claims;
using InventoryPlatform.Application.Interfaces.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace InventoryPlatform.Web.Authorization;

public sealed class MultiCapabilityAuthorizationHandler
    : AuthorizationHandler<MultiCapabilityRequirement>
{
    private readonly ICapabilityAuthorizationService _authorizationService;

    public MultiCapabilityAuthorizationHandler(
        ICapabilityAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MultiCapabilityRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var userIdValue = context.User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return;
        }

        foreach (var capabilityName in requirement.CapabilityNames)
        {
            var hasCapability = await _authorizationService.HasCapabilityAsync(
                userId,
                capabilityName);

            if (hasCapability)
            {
                context.Succeed(requirement);
                return;
            }
        }
    }
}
