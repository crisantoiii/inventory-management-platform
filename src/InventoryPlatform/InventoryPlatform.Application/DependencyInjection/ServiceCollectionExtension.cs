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

using InventoryPlatform.Application.Features.Suppliers.CreateSupplier;
using InventoryPlatform.Application.Features.Suppliers.DeactivateSupplier;
using InventoryPlatform.Application.Features.Suppliers.GetSupplier;
using InventoryPlatform.Application.Features.Suppliers.GetSuppliers;
using InventoryPlatform.Application.Features.Suppliers.UpdateSupplier;
using InventoryPlatform.Application.Features.Suppliers.ActivateSupplier;

using InventoryPlatform.Application.Features.Customers.CreateCustomer;
using InventoryPlatform.Application.Features.Customers.DeactivateCustomer;
using InventoryPlatform.Application.Features.Customers.GetCustomer;
using InventoryPlatform.Application.Features.Customers.GetCustomers;
using InventoryPlatform.Application.Features.Customers.UpdateCustomer;
using InventoryPlatform.Application.Features.Customers.ActivateCustomer;

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

        services.AddScoped<CreateSupplierHandler>();
        services.AddScoped<GetSupplierHandler>();
        services.AddScoped<GetSuppliersHandler>();
        services.AddScoped<UpdateSupplierHandler>();
        services.AddScoped<DeactivateSupplierHandler>();
        services.AddScoped<ActivateSupplierHandler>();

        services.AddScoped<CreateCustomerHandler>();
        services.AddScoped<GetCustomerHandler>();
        services.AddScoped<GetCustomersHandler>();
        services.AddScoped<UpdateCustomerHandler>();
        services.AddScoped<DeactivateCustomerHandler>();
        services.AddScoped<ActivateCustomerHandler>();

        services.AddValidatorsFromAssembly(
            typeof(ServiceCollectionExtensions).Assembly);

        return services;
    }
}