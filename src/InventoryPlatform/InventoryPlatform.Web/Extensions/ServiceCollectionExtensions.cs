using Microsoft.Extensions.DependencyInjection;

namespace InventoryPlatform.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWeb(
        this IServiceCollection services)
    {
        services.AddRazorPages();

        return services;
    }
}