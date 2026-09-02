using System.Security.Claims;
using InventoryPlatform.Application.Interfaces.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace InventoryPlatform.Web.Authorization;

public sealed class CapabilityAuthorizationHandler
    : AuthorizationHandler<CapabilityRequirement>
{
    private readonly ICapabilityAuthorizationService _authorizationService;

    public CapabilityAuthorizationHandler(
        ICapabilityAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CapabilityRequirement requirement)
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

        var hasCapability = await _authorizationService.HasCapabilityAsync(
            userId,
            requirement.CapabilityName);

        if (hasCapability)
        {
            context.Succeed(requirement);
        }
    }
}
