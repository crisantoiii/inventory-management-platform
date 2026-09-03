using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Infrastructure.Persistence.Context;
using InventoryPlatform.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryPlatform.IntegrationTests.Authorization;

public class CapabilityRepositoryTests : IDisposable
{
    private readonly DbContextOptions<ApplicationDbContext> _options;
    private readonly ApplicationDbContext _context;
    private readonly CapabilityRepository _repository;

    public CapabilityRepositoryTests()
    {
        var dbName = $"CapRepoTest_{Guid.NewGuid():N}";
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        _context = new ApplicationDbContext(_options);
        _repository = new CapabilityRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    // --- GetByNameAsync ---

    [Fact]
    public async Task GetByNameAsync_WhenCapabilityExists_ReturnsCapability()
    {
        var capability = new Capability("Product.View");
        _context.Capabilities.Add(capability);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByNameAsync("Product.View");

        Assert.NotNull(result);
        Assert.Equal("Product.View", result.Name);
        Assert.True(result.IsEnabled);
    }

    [Fact]
    public async Task GetByNameAsync_WhenCapabilityDoesNotExist_ReturnsNull()
    {
        var result = await _repository.GetByNameAsync("Nonexistent.Capability");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByNameAsync_ReturnsPersistedId()
    {
        var capability = new Capability("Test.Cap");
        _context.Capabilities.Add(capability);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByNameAsync("Test.Cap");

        Assert.NotNull(result);
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task GetByNameAsync_WithExactCaseMatch_ReturnsCapability()
    {
        _context.Capabilities.Add(new Capability("Product.View"));
        await _context.SaveChangesAsync();

        var result = await _repository.GetByNameAsync("Product.View");

        Assert.NotNull(result);
        Assert.Equal("Product.View", result.Name);
    }

    [Fact]
    public async Task GetByNameAsync_WithDifferentCase_ReturnsNull()
    {
        _context.Capabilities.Add(new Capability("Product.View"));
        await _context.SaveChangesAsync();

        // GetByNameAsync uses == which is case-sensitive
        var result = await _repository.GetByNameAsync("product.view");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByNameAsync_WhenCapabilityIsDisabled_ReturnsDisabledCapability()
    {
        var capability = new Capability("Disabled.Cap");
        capability.Disable();
        _context.Capabilities.Add(capability);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByNameAsync("Disabled.Cap");

        Assert.NotNull(result);
        Assert.False(result.IsEnabled);
    }

    [Fact]
    public async Task GetByNameAsync_MultipleCapabilities_ReturnsCorrectOne()
    {
        _context.Capabilities.Add(new Capability("Product.View"));
        _context.Capabilities.Add(new Capability("Product.Create"));
        _context.Capabilities.Add(new Capability("Category.View"));
        await _context.SaveChangesAsync();

        var result = await _repository.GetByNameAsync("Product.Create");

        Assert.NotNull(result);
        Assert.Equal("Product.Create", result.Name);
    }

    [Fact]
    public async Task GetByNameAsync_DoesNotReturnUnrelatedCapabilities()
    {
        _context.Capabilities.Add(new Capability("Product.View"));
        _context.Capabilities.Add(new Capability("Category.View"));
        await _context.SaveChangesAsync();

        var result = await _repository.GetByNameAsync("Product.View");

        Assert.NotNull(result);
        Assert.Single(_context.Capabilities.Where(c => c.Name == "Product.View"));
    }

    // --- Base Repository: GetByIdAsync ---

    [Fact]
    public async Task GetByIdAsync_WhenCapabilityExists_ReturnsCapability()
    {
        var capability = new Capability("Test.Cap");
        _context.Capabilities.Add(capability);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(capability.Id);

        Assert.NotNull(result);
        Assert.Equal("Test.Cap", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCapabilityDoesNotExist_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(9999);

        Assert.Null(result);
    }

    // --- Base Repository: AddAsync ---

    [Fact]
    public async Task AddAsync_PersistsNewCapability()
    {
        var capability = new Capability("New.Cap");

        await _repository.AddAsync(capability);
        await _context.SaveChangesAsync();

        var persisted = await _context.Capabilities
            .AsNoTracking()
            .SingleAsync(c => c.Name == "New.Cap");

        Assert.True(persisted.Id > 0);
        Assert.Equal("New.Cap", persisted.Name);
    }

    // --- Base Repository: ExistsAsync ---

    [Fact]
    public async Task ExistsAsync_WhenCapabilityExists_ReturnsTrue()
    {
        _context.Capabilities.Add(new Capability("Exist.Cap"));
        await _context.SaveChangesAsync();

        var result = await _repository.ExistsAsync(c => c.Name == "Exist.Cap");

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_WhenCapabilityDoesNotExist_ReturnsFalse()
    {
        var result = await _repository.ExistsAsync(c => c.Name == "Nonexistent");

        Assert.False(result);
    }
}
