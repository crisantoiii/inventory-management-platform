using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace InventoryPlatform.Infrastructure.Identity;

public static class AuthorizationSeeder
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> GroupCapabilities =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
        {
            [IdentityConstants.Roles.Administrator] = CapabilityCatalog.All,
            [IdentityConstants.Roles.InventoryManager] = CapabilityCatalog.InventoryManager,
            [IdentityConstants.Roles.Viewer] = CapabilityCatalog.Viewer
        };

    public static async Task SeedAsync(
        ApplicationDbContext context,
        CancellationToken cancellationToken = default)
    {
        var capabilities = await EnsureCapabilitiesAsync(
            context,
            cancellationToken);

        var groups = await EnsureGroupsAsync(
            context,
            capabilities,
            cancellationToken);

    }

    private static async Task<IReadOnlyDictionary<string, Capability>> EnsureCapabilitiesAsync(
        ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var names = CapabilityCatalog.All
            .ToHashSet(StringComparer.Ordinal);

        var existing = await context.Capabilities
            .Where(x => names.Contains(x.Name))
            .ToDictionaryAsync(
                x => x.Name,
                StringComparer.Ordinal,
                cancellationToken);

        foreach (var name in CapabilityCatalog.All)
        {
            if (existing.ContainsKey(name))
            {
                continue;
            }

            var capability = new Capability(name);
            context.Capabilities.Add(capability);
            existing.Add(name, capability);
        }

        await context.SaveChangesAsync(cancellationToken);

        return existing;
    }

    private static async Task<IReadOnlyDictionary<string, AuthorizationGroup>> EnsureGroupsAsync(
        ApplicationDbContext context,
        IReadOnlyDictionary<string, Capability> capabilities,
        CancellationToken cancellationToken)
    {
        var groupNames = GroupCapabilities.Keys.ToHashSet(StringComparer.Ordinal);

        var groups = await context.AuthorizationGroups
            .Include(x => x.Capabilities)
            .Where(x => groupNames.Contains(x.Name))
            .ToDictionaryAsync(
                x => x.Name,
                StringComparer.Ordinal,
                cancellationToken);

        foreach (var groupName in GroupCapabilities.Keys)
        {
            if (!groups.TryGetValue(groupName, out var group))
            {
                group = new AuthorizationGroup(groupName);
                context.AuthorizationGroups.Add(group);
                await context.SaveChangesAsync(cancellationToken);
                groups.Add(groupName, group);
            }

            foreach (var capabilityName in GroupCapabilities[groupName])
            {
                if (!capabilities.TryGetValue(capabilityName, out var capability))
                {
                    throw new InvalidOperationException(
                        $"Seed capability '{capabilityName}' was not found.");
                }

                if (group.Capabilities.All(
                    relationship => relationship.CapabilityId != capability.Id))
                {
                    group.AddCapability(capability);
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return groups;
    }

    public static IReadOnlyDictionary<string, string> RoleToGroup =>
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [IdentityConstants.Roles.Administrator] = IdentityConstants.Roles.Administrator,
            [IdentityConstants.Roles.InventoryManager] = IdentityConstants.Roles.InventoryManager,
            [IdentityConstants.Roles.Viewer] = IdentityConstants.Roles.Viewer
        };

    private static class CapabilityCatalog
    {
        public static readonly IReadOnlyList<string> All =
        [
            Dashboard.View,

            Product.View,
            Product.Create,
            Product.Edit,
            Product.Activate,
            Product.Deactivate,

            Category.View,
            Category.Create,
            Category.Activate,
            Category.Deactivate,

            Supplier.View,
            Supplier.Create,
            Supplier.Edit,
            Supplier.Activate,
            Supplier.Deactivate,

            Customer.View,
            Customer.Create,
            Customer.Edit,
            Customer.Activate,
            Customer.Deactivate,

            Unit.View,
            Unit.Create,
            Unit.Edit,
            Unit.Activate,
            Unit.Deactivate,

            InventoryTransaction.View,
            InventoryTransaction.Create,

            User.View,
            User.Create,
            User.Edit,
            User.EditRoles,
            User.ResetPassword,
            User.EditStatus,

            PurchaseOrder.View,
            PurchaseOrder.Create,
            PurchaseOrder.Submit,
            PurchaseOrder.Approve,
            PurchaseOrder.Receive
        ];

        public static readonly IReadOnlyList<string> InventoryManager =
            All
                .Where(name =>
                    !name.StartsWith("User.", StringComparison.Ordinal) &&
                    name is not Product.Activate &&
                    name is not Product.Deactivate &&
                    name is not Category.Activate &&
                    name is not Category.Deactivate &&
                    name is not Supplier.Activate &&
                    name is not Supplier.Deactivate &&
                    name is not Unit.Create &&
                    name is not Unit.Activate &&
                    name is not Unit.Deactivate &&
                    name is not Customer.Activate &&
                    name is not Customer.Deactivate)
                .ToArray();

        public static readonly IReadOnlyList<string> Viewer =
            All
                .Where(name =>
                    name.EndsWith(".View", StringComparison.Ordinal) ||
                    name == Supplier.Create ||
                    name.StartsWith("PurchaseOrder.", StringComparison.Ordinal))
                .ToArray();

        private static class Dashboard
        {
            public const string View = "Dashboard.View";
        }

        private static class Product
        {
            public const string View = "Product.View";
            public const string Create = "Product.Create";
            public const string Edit = "Product.Edit";
            public const string Activate = "Product.Activate";
            public const string Deactivate = "Product.Deactivate";
        }

        private static class Category
        {
            public const string View = "Category.View";
            public const string Create = "Category.Create";
            public const string Activate = "Category.Activate";
            public const string Deactivate = "Category.Deactivate";
        }

        private static class Supplier
        {
            public const string View = "Supplier.View";
            public const string Create = "Supplier.Create";
            public const string Edit = "Supplier.Edit";
            public const string Activate = "Supplier.Activate";
            public const string Deactivate = "Supplier.Deactivate";
        }

        private static class Customer
        {
            public const string View = "Customer.View";
            public const string Create = "Customer.Create";
            public const string Edit = "Customer.Edit";
            public const string Activate = "Customer.Activate";
            public const string Deactivate = "Customer.Deactivate";
        }

        private static class Unit
        {
            public const string View = "Unit.View";
            public const string Create = "Unit.Create";
            public const string Edit = "Unit.Edit";
            public const string Activate = "Unit.Activate";
            public const string Deactivate = "Unit.Deactivate";
        }

        private static class InventoryTransaction
        {
            public const string View = "InventoryTransaction.View";
            public const string Create = "InventoryTransaction.Create";
        }

        private static class User
        {
            public const string View = "User.View";
            public const string Create = "User.Create";
            public const string Edit = "User.Edit";
            public const string EditRoles = "User.EditRoles";
            public const string ResetPassword = "User.ResetPassword";
            public const string EditStatus = "User.EditStatus";
        }

        private static class PurchaseOrder
        {
            public const string View = "PurchaseOrder.View";
            public const string Create = "PurchaseOrder.Create";
            public const string Submit = "PurchaseOrder.Submit";
            public const string Approve = "PurchaseOrder.Approve";
            public const string Receive = "PurchaseOrder.Receive";
        }
    }
}
