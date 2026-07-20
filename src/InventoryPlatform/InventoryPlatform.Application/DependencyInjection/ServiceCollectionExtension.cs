using FluentValidation;

using InventoryPlatform.Application.Features.Products.CreateProduct;
using InventoryPlatform.Application.Features.Products.DeactivateProduct;
using InventoryPlatform.Application.Features.Products.GetProduct;
using InventoryPlatform.Application.Features.Products.GetProducts;
using InventoryPlatform.Application.Features.Products.UpdateProduct;
using InventoryPlatform.Application.Features.Products.ActivateProduct;

using InventoryPlatform.Application.Features.Categories.CreateCategory;
using InventoryPlatform.Application.Features.Categories.DeactivateCategory;
using InventoryPlatform.Application.Features.Categories.GetCategory;
using InventoryPlatform.Application.Features.Categories.GetCategories;
using InventoryPlatform.Application.Features.Categories.UpdateCategory;
using InventoryPlatform.Application.Features.Categories.ActivateCategory;

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
        services.AddScoped<ActivateProductHandler>();

        services.AddScoped<CreateCategoryHandler>();
        services.AddScoped<GetCategoryHandler>();
        services.AddScoped<GetCategoriesHandler>();
        services.AddScoped<UpdateCategoryHandler>();
        services.AddScoped<DeactivateCategoryHandler>();
        services.AddScoped<ActivateCategoryHandler>();


        services.AddValidatorsFromAssembly(
            typeof(ServiceCollectionExtensions).Assembly);

        return services;
    }
}