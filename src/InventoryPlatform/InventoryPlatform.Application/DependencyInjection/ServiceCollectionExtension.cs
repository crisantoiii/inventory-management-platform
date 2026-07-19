using FluentValidation;
using InventoryPlatform.Application.Features.Products.CreateProduct;
using InventoryPlatform.Application.Features.Products.DeactivateProduct;
using InventoryPlatform.Application.Features.Products.GetProduct;
using InventoryPlatform.Application.Features.Products.GetProducts;
using InventoryPlatform.Application.Features.Products.UpdateProduct;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryPlatform.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<CreateProductHandler>();
        services.AddScoped<GetProductHandler>();
        services.AddScoped<GetProductsHandler>();
        services.AddScoped<UpdateProductHandler>();
        services.AddScoped<DeactivateProductHandler>();

        services.AddValidatorsFromAssembly(
            typeof(ServiceCollectionExtensions).Assembly);

        return services;
    }
}