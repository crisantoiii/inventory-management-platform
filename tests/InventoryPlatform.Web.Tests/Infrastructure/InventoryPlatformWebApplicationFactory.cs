using InventoryPlatform.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace InventoryPlatform.Web.Tests.Infrastructure;

public sealed class InventoryPlatformWebApplicationFactory
    : WebApplicationFactory<Program>
{
    public const string StructuralValidationFailureMessage =
        "Unsafe ApplicationDbContext registration survived test-host replacement.";

    private const string SentinelConnectionString =
        "T03_SENTINEL_NOT_A_SQL_CONNECTION_STRING";

    private readonly bool _preserveProductionConfigurationForStructuralFailure;

    public InventoryPlatformWebApplicationFactory(
        bool preserveProductionConfigurationForStructuralFailure = false)
    {
        _preserveProductionConfigurationForStructuralFailure =
            preserveProductionConfigurationForStructuralFailure;
    }

    public string DatabaseName { get; } =
        $"InventoryPlatformHttpTests_{Guid.NewGuid():N}";

    public string ExpectedSentinelConnectionString => SentinelConnectionString;

    public IReadOnlyList<string> CapturedContextDescriptorTypes { get; private set; } = [];

    public bool StructuralValidationReached { get; private set; }

    public bool StructuralValidationSucceeded { get; private set; }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(configuration =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = SentinelConnectionString
                });
        });

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var capturedDescriptors = services
                .Where(IsApplicationDbContextDescriptor)
                .ToArray();

            CapturedContextDescriptorTypes = capturedDescriptors
                .Select(descriptor => descriptor.ServiceType.FullName ?? descriptor.ServiceType.Name)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();

            foreach (var descriptor in capturedDescriptors)
            {
                services.Remove(descriptor);
            }

            services.RemoveAll<ApplicationDbContext>();
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(DatabaseName));

            if (_preserveProductionConfigurationForStructuralFailure)
            {
                var productionConfiguration = capturedDescriptors.Single(
                    descriptor => descriptor.ServiceType ==
                        typeof(IDbContextOptionsConfiguration<ApplicationDbContext>));

                services.Add(productionConfiguration);
            }

            StructuralValidationReached = true;
            ValidateContextRegistrationStructure(services, capturedDescriptors);
            StructuralValidationSucceeded = true;
        });
    }

    private static bool IsApplicationDbContextDescriptor(
        ServiceDescriptor descriptor)
    {
        return descriptor.ServiceType == typeof(ApplicationDbContext) ||
               descriptor.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
               descriptor.ServiceType ==
                   typeof(IDbContextOptionsConfiguration<ApplicationDbContext>);
    }

    private static void ValidateContextRegistrationStructure(
        IServiceCollection services,
        IReadOnlyCollection<ServiceDescriptor> capturedDescriptors)
    {
        var capturedDescriptorSurvives = capturedDescriptors.Any(
            captured => services.Any(current => ReferenceEquals(current, captured)));

        var contextCount = services.Count(
            descriptor => descriptor.ServiceType == typeof(ApplicationDbContext));

        var optionsCount = services.Count(
            descriptor => descriptor.ServiceType ==
                typeof(DbContextOptions<ApplicationDbContext>));

        var configurationCount = services.Count(
            descriptor => descriptor.ServiceType ==
                typeof(IDbContextOptionsConfiguration<ApplicationDbContext>));

        if (capturedDescriptorSurvives ||
            contextCount != 1 ||
            optionsCount != 1 ||
            configurationCount != 1)
        {
            throw new InvalidOperationException(
                StructuralValidationFailureMessage);
        }
    }
}
