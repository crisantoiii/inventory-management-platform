using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Infrastructure.Identity;
using InventoryPlatform.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InventoryPlatform.Web.Tests.Infrastructure;

public sealed class InventoryPlatformWebApplicationFactoryTests
{
    [Fact]
    public async Task Services_WhenFactoryStarts_UseSentinelAndIsolatedInMemoryProvider()
    {
        await using var factory = new InventoryPlatformWebApplicationFactory();

        var services = factory.Services;
        var configuration = services.GetRequiredService<IConfiguration>();
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Assert.Equal(
            factory.ExpectedSentinelConnectionString,
            configuration.GetConnectionString("DefaultConnection"));
        Assert.Equal("Microsoft.EntityFrameworkCore.InMemory", context.Database.ProviderName);
        Assert.NotEqual("Microsoft.EntityFrameworkCore.SqlServer", context.Database.ProviderName);
        Assert.True(factory.StructuralValidationReached);
        Assert.True(factory.StructuralValidationSucceeded);
    }

    [Fact]
    public async Task Services_WhenFactoryStarts_PreserveRealStartupSeedBaseline()
    {
        await using var factory = new InventoryPlatformWebApplicationFactory();
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Assert.True(await context.Users.AnyAsync(
            user => user.Email == IdentityConstants.DefaultAdmin.Email));
        Assert.True(await context.Users.AnyAsync(
            user => user.Email == IdentityConstants.DefaultManager.Email));
        Assert.True(await context.Users.AnyAsync(
            user => user.Email == IdentityConstants.DefaultViewer.Email));
        Assert.True(await context.Capabilities.AnyAsync(
            capability => capability.Name == "Category.Create"));
        Assert.True(await context.AuthorizationGroups.AnyAsync(
            group => group.Name == IdentityConstants.Roles.InventoryManager));
        Assert.True(await context.AuthorizationGroups.AnyAsync(
            group => group.Name == IdentityConstants.Roles.Viewer));
        Assert.True(await context.UserAuthorizationGroups.AnyAsync());
    }

    [Fact]
    public async Task Services_WhenFactoryStarts_ReplacesActualProductionContextDescriptors()
    {
        await using var factory = new InventoryPlatformWebApplicationFactory();

        _ = factory.Services;

        Assert.Contains(
            typeof(ApplicationDbContext).FullName!,
            factory.CapturedContextDescriptorTypes);
        Assert.Contains(
            typeof(DbContextOptions<ApplicationDbContext>).FullName!,
            factory.CapturedContextDescriptorTypes);
        Assert.Contains(
            "Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptionsConfiguration`1[[" +
            typeof(ApplicationDbContext).AssemblyQualifiedName + "]]",
            factory.CapturedContextDescriptorTypes);
        Assert.True(factory.StructuralValidationSucceeded);
    }

    [Fact]
    public async Task Services_WhenProductionConfigurationSurvives_FailsStructuralValidation()
    {
        await using var factory = new InventoryPlatformWebApplicationFactory(
            preserveProductionConfigurationForStructuralFailure: true);

        var exception = Assert.Throws<InvalidOperationException>(
            () => _ = factory.Services);

        Assert.Contains(
            InventoryPlatformWebApplicationFactory.StructuralValidationFailureMessage,
            exception.ToString(),
            StringComparison.Ordinal);
        Assert.True(factory.StructuralValidationReached);
        Assert.False(factory.StructuralValidationSucceeded);
    }

    [Fact]
    public async Task Factories_WhenStarted_OwnDistinctIsolatedSeededDatabases()
    {
        await using var factoryA = new InventoryPlatformWebApplicationFactory();
        await using var factoryB = new InventoryPlatformWebApplicationFactory();

        Assert.NotEqual(factoryA.DatabaseName, factoryB.DatabaseName);

        var categoryName = $"FactoryA-{Guid.NewGuid():N}";

        await using (var scopeA = factoryA.Services.CreateAsyncScope())
        {
            var contextA = scopeA.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            contextA.Categories.Add(new Category(categoryName, "Isolation marker"));
            await contextA.SaveChangesAsync();

            Assert.True(await contextA.Users.AnyAsync(
                user => user.Email == IdentityConstants.DefaultManager.Email));
        }

        await using (var scopeB = factoryB.Services.CreateAsyncScope())
        {
            var contextB = scopeB.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            Assert.True(await contextB.Users.AnyAsync(
                user => user.Email == IdentityConstants.DefaultManager.Email));
            Assert.False(await contextB.Categories.AnyAsync(
                category => category.Name == categoryName));
        }
    }
}
