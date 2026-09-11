using InventoryPlatform.Web.Authorization;
using InventoryPlatform.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace InventoryPlatform.Web.Tests.Authorization;

/// <summary>
/// Sprint 14 T05: verifies that the new PurchaseOrder.Edit and
/// PurchaseOrder.Cancel capabilities follow the existing singular
/// PurchaseOrder.Action convention and that their authorization policies
/// resolve from the real AddWeb registration (AuthorizationOptions) as
/// CapabilityRequirement instances carrying exactly the expected capability
/// name, using the established "Capability:&lt;capability&gt;" policy naming.
/// </summary>
public class PurchaseOrderCapabilityPolicyRegistrationTests
{
    // --- Capability catalog constants ---

    [Fact]
    public void PurchaseOrder_EditCapability_ExistsWithSingularNamingConvention()
    {
        Assert.Equal("PurchaseOrder.Edit", AuthorizationPolicies.PurchaseOrder.Edit);
    }

    [Fact]
    public void PurchaseOrder_CancelCapability_ExistsWithSingularNamingConvention()
    {
        Assert.Equal("PurchaseOrder.Cancel", AuthorizationPolicies.PurchaseOrder.Cancel);
    }

    [Fact]
    public void PurchaseOrder_EditPolicyConstant_FollowsCapabilityPolicyConvention()
    {
        Assert.Equal(
            "Capability:PurchaseOrder.Edit",
            AuthorizationPolicies.PurchaseOrder.EditPolicy);
    }

    [Fact]
    public void PurchaseOrder_CancelPolicyConstant_FollowsCapabilityPolicyConvention()
    {
        Assert.Equal(
            "Capability:PurchaseOrder.Cancel",
            AuthorizationPolicies.PurchaseOrder.CancelPolicy);
    }

    // --- Policy registration (real AddWeb registration) ---

    [Fact]
    public void AddWeb_RegistersEditPolicy_WithSingleCapabilityRequirement()
    {
        var options = BuildRegisteredOptions();

        var policy = options.GetPolicy(
            AuthorizationPolicies.ForCapability(AuthorizationPolicies.PurchaseOrder.Edit));

        Assert.NotNull(policy);
        var requirement = Assert.Single(policy!.Requirements.OfType<CapabilityRequirement>());
        Assert.Equal(AuthorizationPolicies.PurchaseOrder.Edit, requirement.CapabilityName);
    }

    [Fact]
    public void AddWeb_RegistersCancelPolicy_WithSingleCapabilityRequirement()
    {
        var options = BuildRegisteredOptions();

        var policy = options.GetPolicy(
            AuthorizationPolicies.ForCapability(AuthorizationPolicies.PurchaseOrder.Cancel));

        Assert.NotNull(policy);
        var requirement = Assert.Single(policy!.Requirements.OfType<CapabilityRequirement>());
        Assert.Equal(AuthorizationPolicies.PurchaseOrder.Cancel, requirement.CapabilityName);
    }

    private static AuthorizationOptions BuildRegisteredOptions()
    {
        var services = new ServiceCollection();
        services.AddWeb();

        using var provider = services.BuildServiceProvider();

        // Create the options exactly as the runtime would: run every registered
        // IConfigureOptions<AuthorizationOptions>, including the AddWeb
        // AddAuthorization configuration.
        var optionsFactory = provider.GetRequiredService<IOptionsFactory<AuthorizationOptions>>();
        return optionsFactory.Create(Options.DefaultName);
    }
}
