using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Infrastructure.Identity;
using InventoryPlatform.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryPlatform.IntegrationTests.Authorization;

public class AuthorizationSeederTests : IDisposable
{
    private readonly DbContextOptions<ApplicationDbContext> _options;
    private readonly ApplicationDbContext _context;

    public AuthorizationSeederTests()
    {
        var dbName = $"SeederTest_{Guid.NewGuid():N}";
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        _context = new ApplicationDbContext(_options);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    // --- Empty Database Seeding ---

    [Fact]
    public async Task SeedAsync_WhenDatabaseIsEmpty_SeedsExpectedCapabilities()
    {
        await AuthorizationSeeder.SeedAsync(_context);

        var capabilities = await _context.Capabilities
            .AsNoTracking()
            .ToListAsync();

        Assert.Equal(41, capabilities.Count);
    }

    [Fact]
    public async Task SeedAsync_WhenDatabaseIsEmpty_SeedsExpectedAuthorizationGroups()
    {
        await AuthorizationSeeder.SeedAsync(_context);

        var groups = await _context.AuthorizationGroups
            .AsNoTracking()
            .ToListAsync();

        Assert.Equal(3, groups.Count);
        Assert.Contains(groups, g => g.Name == "Administrator");
        Assert.Contains(groups, g => g.Name == "InventoryManager");
        Assert.Contains(groups, g => g.Name == "Viewer");
    }

    [Fact]
    public async Task SeedAsync_WhenDatabaseIsEmpty_SeedsExpectedGroupCapabilityRelationships()
    {
        await AuthorizationSeeder.SeedAsync(_context);

        var relationships = await _context.AuthorizationGroupCapabilities
            .AsNoTracking()
            .ToListAsync();

        Assert.Equal(79, relationships.Count);
    }

    // --- Capability Names ---

    [Fact]
    public async Task SeedAsync_SeedsAllExpectedCapabilityNames()
    {
        await AuthorizationSeeder.SeedAsync(_context);

        var capabilityNames = await _context.Capabilities
            .AsNoTracking()
            .Select(c => c.Name)
            .ToListAsync();

        var expectedNames = new[]
        {
            "Administration.Access",
            "Dashboard.View",
            "Product.View", "Product.Create", "Product.Edit", "Product.Activate", "Product.Deactivate",
            "Category.View", "Category.Create", "Category.Activate", "Category.Deactivate",
            "Supplier.View", "Supplier.Create", "Supplier.Edit", "Supplier.Activate", "Supplier.Deactivate",
            "Customer.View", "Customer.Create", "Customer.Edit", "Customer.Activate", "Customer.Deactivate",
            "Unit.View", "Unit.Create", "Unit.Edit", "Unit.Activate", "Unit.Deactivate",
            "InventoryTransaction.View", "InventoryTransaction.Create",
            "User.View", "User.Create", "User.Edit", "User.EditRoles", "User.ResetPassword", "User.EditStatus",
            "PurchaseOrder.View", "PurchaseOrder.Create", "PurchaseOrder.Edit", "PurchaseOrder.Submit", "PurchaseOrder.Approve", "PurchaseOrder.Receive", "PurchaseOrder.Cancel"
        };

        Assert.Equal(expectedNames.Length, capabilityNames.Count);
        foreach (var name in expectedNames)
        {
            Assert.Contains(name, capabilityNames);
        }
    }

    // --- Capability Enabled State ---

    [Fact]
    public async Task SeedAsync_AllSeededCapabilitiesAreEnabled()
    {
        await AuthorizationSeeder.SeedAsync(_context);

        var capabilities = await _context.Capabilities
            .AsNoTracking()
            .ToListAsync();

        Assert.All(capabilities, c => Assert.True(c.IsEnabled));
    }

    // --- Administrator Coverage ---

    [Fact]
    public async Task SeedAsync_AdministratorReceivesAll41Capabilities()
    {
        await AuthorizationSeeder.SeedAsync(_context);

        var adminGroup = await GetGroupWithCapabilities("Administrator");

        Assert.Equal(41, adminGroup.Capabilities.Count);
    }

    [Fact]
    public async Task SeedAsync_AdministratorReceivesAdministrationAccess()
    {
        await AuthorizationSeeder.SeedAsync(_context);

        var adminGroup = await GetGroupWithCapabilities("Administrator");
        var adminAccess = await GetCapabilityId("Administration.Access");

        Assert.Contains(adminGroup.Capabilities, r => r.CapabilityId == adminAccess);
    }

    [Fact]
    public async Task SeedAsync_AdministratorReceivesAllProductCapabilities()
    {
        await AuthorizationSeeder.SeedAsync(_context);

        var adminGroup = await GetGroupWithCapabilities("Administrator");

        var productCapabilities = new[] { "Product.View", "Product.Create", "Product.Edit", "Product.Activate", "Product.Deactivate" };
        foreach (var name in productCapabilities)
        {
            var capId = await GetCapabilityId(name);
            Assert.Contains(adminGroup.Capabilities, r => r.CapabilityId == capId);
        }
    }

    [Fact]
    public async Task SeedAsync_AdministratorReceivesAllPurchaseOrderCapabilities()
    {
        await AuthorizationSeeder.SeedAsync(_context);

        var adminGroup = await GetGroupWithCapabilities("Administrator");

        var poCapabilities = new[] { "PurchaseOrder.View", "PurchaseOrder.Create", "PurchaseOrder.Edit", "PurchaseOrder.Submit", "PurchaseOrder.Approve", "PurchaseOrder.Receive", "PurchaseOrder.Cancel" };
        foreach (var name in poCapabilities)
        {
            var capId = await GetCapabilityId(name);
            Assert.Contains(adminGroup.Capabilities, r => r.CapabilityId == capId);
        }
    }

    // --- InventoryManager Coverage ---

    [Fact]
    public async Task SeedAsync_InventoryManagerReceives23Capabilities()
    {
        await AuthorizationSeeder.SeedAsync(_context);

        var managerGroup = await GetGroupWithCapabilities("InventoryManager");

        Assert.Equal(23, managerGroup.Capabilities.Count);
    }

    [Fact]
    public async Task SeedAsync_InventoryManagerDoesNotReceiveUserCapabilities()
    {
        await AuthorizationSeeder.SeedAsync(_context);

        var managerGroup = await GetGroupWithCapabilities("InventoryManager");

        var userCapabilities = new[] { "User.View", "User.Create", "User.Edit", "User.EditRoles", "User.ResetPassword", "User.EditStatus" };
        foreach (var name in userCapabilities)
        {
            var capId = await GetCapabilityId(name);
            Assert.DoesNotContain(managerGroup.Capabilities, r => r.CapabilityId == capId);
        }
    }

    [Fact]
    public async Task SeedAsync_InventoryManagerDoesNotReceiveAdministrationAccess()
    {
        await AuthorizationSeeder.SeedAsync(_context);

        var managerGroup = await GetGroupWithCapabilities("InventoryManager");
        var adminAccess = await GetCapabilityId("Administration.Access");

        Assert.DoesNotContain(managerGroup.Capabilities, r => r.CapabilityId == adminAccess);
    }

    [Fact]
    public async Task SeedAsync_InventoryManagerDoesNotReceiveActivationCapabilities()
    {
        await AuthorizationSeeder.SeedAsync(_context);

        var managerGroup = await GetGroupWithCapabilities("InventoryManager");

        var activationCapabilities = new[]
        {
            "Product.Activate", "Product.Deactivate",
            "Category.Activate", "Category.Deactivate",
            "Supplier.Activate", "Supplier.Deactivate",
            "Unit.Activate", "Unit.Deactivate",
            "Customer.Activate", "Customer.Deactivate"
        };

        foreach (var name in activationCapabilities)
        {
            var capId = await GetCapabilityId(name);
            Assert.DoesNotContain(managerGroup.Capabilities, r => r.CapabilityId == capId);
        }
    }

    [Fact]
    public async Task SeedAsync_InventoryManagerDoesNotReceiveUnitCreate()
    {
        await AuthorizationSeeder.SeedAsync(_context);

        var managerGroup = await GetGroupWithCapabilities("InventoryManager");
        var unitCreate = await GetCapabilityId("Unit.Create");

        Assert.DoesNotContain(managerGroup.Capabilities, r => r.CapabilityId == unitCreate);
    }

    [Fact]
    public async Task SeedAsync_InventoryManagerReceivesPurchaseOrderEdit()
    {
        await AuthorizationSeeder.SeedAsync(_context);

        var managerGroup = await GetGroupWithCapabilities("InventoryManager");
        var poEdit = await GetCapabilityId("PurchaseOrder.Edit");

        Assert.Contains(managerGroup.Capabilities, r => r.CapabilityId == poEdit);
    }

    [Fact]
    public async Task SeedAsync_InventoryManagerReceivesPurchaseOrderCancel()
    {
        await AuthorizationSeeder.SeedAsync(_context);

        var managerGroup = await GetGroupWithCapabilities("InventoryManager");
        var poCancel = await GetCapabilityId("PurchaseOrder.Cancel");

        Assert.Contains(managerGroup.Capabilities, r => r.CapabilityId == poCancel);
    }

    [Fact]
    public async Task SeedAsync_InventoryManagerReceivesDashboardView()
    {
        await AuthorizationSeeder.SeedAsync(_context);

        var managerGroup = await GetGroupWithCapabilities("InventoryManager");
        var dashboardView = await GetCapabilityId("Dashboard.View");

        Assert.Contains(managerGroup.Capabilities, r => r.CapabilityId == dashboardView);
    }

    // --- Viewer Coverage ---

    [Fact]
    public async Task SeedAsync_ViewerReceives15Capabilities()
    {
        await AuthorizationSeeder.SeedAsync(_context);

        var viewerGroup = await GetGroupWithCapabilities("Viewer");

        // 7 .View capabilities + 7 PurchaseOrder.* + User.View (ends with .View) = 15
        Assert.Equal(15, viewerGroup.Capabilities.Count);
    }

    [Fact]
    public async Task SeedAsync_ViewerReceivesAllViewCapabilities()
    {
        await AuthorizationSeeder.SeedAsync(_context);

        var viewerGroup = await GetGroupWithCapabilities("Viewer");

        var viewCapabilities = new[]
        {
            "Dashboard.View", "Product.View", "Category.View",
            "Supplier.View", "Customer.View", "Unit.View",
            "InventoryTransaction.View"
        };

        foreach (var name in viewCapabilities)
        {
            var capId = await GetCapabilityId(name);
            Assert.Contains(viewerGroup.Capabilities, r => r.CapabilityId == capId);
        }
    }

    [Fact]
    public async Task SeedAsync_ViewerReceivesAllPurchaseOrderCapabilities()
    {
        await AuthorizationSeeder.SeedAsync(_context);

        var viewerGroup = await GetGroupWithCapabilities("Viewer");

        var poCapabilities = new[] { "PurchaseOrder.View", "PurchaseOrder.Create", "PurchaseOrder.Edit", "PurchaseOrder.Submit", "PurchaseOrder.Approve", "PurchaseOrder.Receive", "PurchaseOrder.Cancel" };
        foreach (var name in poCapabilities)
        {
            var capId = await GetCapabilityId(name);
            Assert.Contains(viewerGroup.Capabilities, r => r.CapabilityId == capId);
        }
    }

    [Fact]
    public async Task SeedAsync_ViewerDoesNotReceiveCreateOrEditCapabilities()
    {
        await AuthorizationSeeder.SeedAsync(_context);

        var viewerGroup = await GetGroupWithCapabilities("Viewer");

        var nonViewCapabilities = new[]
        {
            "Product.Create", "Product.Edit", "Product.Activate", "Product.Deactivate",
            "Category.Create", "Category.Activate", "Category.Deactivate",
            "Supplier.Create", "Supplier.Edit", "Supplier.Activate", "Supplier.Deactivate",
            "Customer.Create", "Customer.Edit", "Customer.Activate", "Customer.Deactivate",
            "Unit.Create", "Unit.Edit", "Unit.Activate", "Unit.Deactivate",
            "InventoryTransaction.Create",
            "User.Create", "User.Edit", "User.EditRoles", "User.ResetPassword", "User.EditStatus",
            "Administration.Access"
        };

        foreach (var name in nonViewCapabilities)
        {
            var capId = await GetCapabilityId(name);
            Assert.DoesNotContain(viewerGroup.Capabilities, r => r.CapabilityId == capId);
        }
    }

    // --- Idempotency ---

    [Fact]
    public async Task SeedAsync_WhenExecutedTwice_DoesNotDuplicateCapabilities()
    {
        await AuthorizationSeeder.SeedAsync(_context);
        await AuthorizationSeeder.SeedAsync(_context);

        var capabilities = await _context.Capabilities
            .AsNoTracking()
            .ToListAsync();

        Assert.Equal(41, capabilities.Count);
    }

    [Fact]
    public async Task SeedAsync_WhenExecutedTwice_DoesNotDuplicateGroups()
    {
        await AuthorizationSeeder.SeedAsync(_context);
        await AuthorizationSeeder.SeedAsync(_context);

        var groups = await _context.AuthorizationGroups
            .AsNoTracking()
            .ToListAsync();

        Assert.Equal(3, groups.Count);
    }

    [Fact]
    public async Task SeedAsync_WhenExecutedTwice_DoesNotDuplicateRelationships()
    {
        await AuthorizationSeeder.SeedAsync(_context);
        await AuthorizationSeeder.SeedAsync(_context);

        var relationships = await _context.AuthorizationGroupCapabilities
            .AsNoTracking()
            .ToListAsync();

        Assert.Equal(79, relationships.Count);
    }

    [Fact]
    public async Task SeedAsync_WhenExecutedTwice_MaintainsExpectedState()
    {
        await AuthorizationSeeder.SeedAsync(_context);
        await AuthorizationSeeder.SeedAsync(_context);

        var adminGroup = await GetGroupWithCapabilities("Administrator");
        Assert.Equal(41, adminGroup.Capabilities.Count);

        var managerGroup = await GetGroupWithCapabilities("InventoryManager");
        Assert.Equal(23, managerGroup.Capabilities.Count);

        var viewerGroup = await GetGroupWithCapabilities("Viewer");
        Assert.Equal(15, viewerGroup.Capabilities.Count);
    }

    // --- Already Seeded State ---

    [Fact]
    public async Task SeedAsync_WhenCapabilitiesAlreadyExist_DoesNotRecreate()
    {
        // Pre-populate one capability
        _context.Capabilities.Add(new Capability("PreExisting.Cap"));
        await _context.SaveChangesAsync();

        await AuthorizationSeeder.SeedAsync(_context);

        var capabilities = await _context.Capabilities
            .AsNoTracking()
            .ToListAsync();

        // 41 seeded + 1 pre-existing = 42
        Assert.Equal(42, capabilities.Count);
        Assert.Contains(capabilities, c => c.Name == "PreExisting.Cap");
    }

    // --- Helper Methods ---

    private async Task<AuthorizationGroup> GetGroupWithCapabilities(string groupName)
    {
        return await _context.AuthorizationGroups
            .Include(g => g.Capabilities)
            .SingleAsync(g => g.Name == groupName);
    }

    private async Task<int> GetCapabilityId(string capabilityName)
    {
        var capability = await _context.Capabilities
            .SingleAsync(c => c.Name == capabilityName);
        return capability.Id;
    }
}
