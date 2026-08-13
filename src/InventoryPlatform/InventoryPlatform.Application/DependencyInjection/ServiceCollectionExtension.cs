using FluentValidation;
using InventoryPlatform.Application.Features.Account;
using InventoryPlatform.Application.Features.Account.ChangePassword;
using InventoryPlatform.Application.Features.Account.ConfirmEmail;
using InventoryPlatform.Application.Features.Account.DisableTwoFactor;
using InventoryPlatform.Application.Features.Account.ForgotPassword;
using InventoryPlatform.Application.Features.Account.GenerateTwoFactorRecoveryCodes;
using InventoryPlatform.Application.Features.Account.GetProfile;
using InventoryPlatform.Application.Features.Account.GetTwoFactorStatus;
using InventoryPlatform.Application.Features.Account.RegenerateTwoFactorRecoveryCodes;
using InventoryPlatform.Application.Features.Account.RequestEmailVerification;
using InventoryPlatform.Application.Features.Account.SetupTwoFactor;
using InventoryPlatform.Application.Features.Account.UpdateProfile;
using InventoryPlatform.Application.Features.Account.VerifyTwoFactor;
using InventoryPlatform.Application.Features.Categories.ActivateCategory;
using InventoryPlatform.Application.Features.Categories.CreateCategory;
using InventoryPlatform.Application.Features.Categories.DeactivateCategory;
using InventoryPlatform.Application.Features.Categories.GetCategories;
using InventoryPlatform.Application.Features.Categories.GetCategory;
using InventoryPlatform.Application.Features.Categories.UpdateCategory;
using InventoryPlatform.Application.Features.Customers.ActivateCustomer;
using InventoryPlatform.Application.Features.Customers.CreateCustomer;
using InventoryPlatform.Application.Features.Customers.DeactivateCustomer;
using InventoryPlatform.Application.Features.Customers.GetCustomer;
using InventoryPlatform.Application.Features.Customers.GetCustomers;
using InventoryPlatform.Application.Features.Customers.UpdateCustomer;
using InventoryPlatform.Application.Features.Dashboard.GetDashboard;
using InventoryPlatform.Application.Features.InventoryTransactions.CreateInventoryTransaction;
using InventoryPlatform.Application.Features.InventoryTransactions.GetInventoryTransaction;
using InventoryPlatform.Application.Features.InventoryTransactions.GetInventoryTransactions;
using InventoryPlatform.Application.Features.Products.ActivateProduct;
using InventoryPlatform.Application.Features.Products.CreateProduct;
using InventoryPlatform.Application.Features.Products.DeactivateProduct;
using InventoryPlatform.Application.Features.Products.GetProduct;
using InventoryPlatform.Application.Features.Products.GetProducts;
using InventoryPlatform.Application.Features.Products.UpdateProduct;
using InventoryPlatform.Application.Features.Purchasing.ApprovePurchaseOrder;
using InventoryPlatform.Application.Features.Purchasing.CreatePurchaseOrder;
using InventoryPlatform.Application.Features.Purchasing.GetPurchaseOrder;
using InventoryPlatform.Application.Features.Purchasing.GetPurchaseOrders;
using InventoryPlatform.Application.Features.Purchasing.ReceivePurchaseOrder;
using InventoryPlatform.Application.Features.Purchasing.SubmitPurchaseOrder;
using InventoryPlatform.Application.Features.Reporting.GetInventoryValuation;
using InventoryPlatform.Application.Features.Suppliers.ActivateSupplier;
using InventoryPlatform.Application.Features.Suppliers.CreateSupplier;
using InventoryPlatform.Application.Features.Suppliers.DeactivateSupplier;
using InventoryPlatform.Application.Features.Suppliers.GetSupplier;
using InventoryPlatform.Application.Features.Suppliers.GetSuppliers;
using InventoryPlatform.Application.Features.Suppliers.UpdateSupplier;
using InventoryPlatform.Application.Features.Units.ActivateUnit;
using InventoryPlatform.Application.Features.Units.CreateUnit;
using InventoryPlatform.Application.Features.Units.DeactivateUnit;
using InventoryPlatform.Application.Features.Units.GetUnit;
using InventoryPlatform.Application.Features.Units.GetUnits;
using InventoryPlatform.Application.Features.Units.UpdateUnit;
using InventoryPlatform.Application.Features.Users.CreateUser;
using InventoryPlatform.Application.Features.Users.GetRoles;
using InventoryPlatform.Application.Features.Users.GetUser;
using InventoryPlatform.Application.Features.Users.GetUsers;
using InventoryPlatform.Application.Features.Users.UpdateUser;
using InventoryPlatform.Application.Features.Users.UpdateUserRoles;
using InventoryPlatform.Application.Features.Users.UpdateUserStatus;
using Microsoft.Extensions.DependencyInjection;
using AccountResetPassword = InventoryPlatform.Application.Features.Account.ResetPassword;
using UserResetPassword = InventoryPlatform.Application.Features.Users.ResetPassword;

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

        services.AddScoped<CreateUnitHandler>();
        services.AddScoped<GetUnitHandler>();
        services.AddScoped<GetUnitsHandler>();
        services.AddScoped<UpdateUnitHandler>();
        services.AddScoped<DeactivateUnitHandler>();
        services.AddScoped<ActivateUnitHandler>();

        services.AddScoped<CreateInventoryTransactionHandler>();
        services.AddScoped<GetInventoryTransactionHandler>();
        services.AddScoped<GetInventoryTransactionsHandler>();

        services.AddScoped<GetDashboardHandler>();

        services.AddScoped<GetUsersHandler>();
        services.AddScoped<GetUserHandler>();
        services.AddScoped<CreateUserHandler>();
        services.AddScoped<GetRolesHandler>();
        services.AddScoped<UpdateUserHandler>();
        services.AddScoped<UpdateUserRolesHandler>();
        services.AddScoped<UpdateUserStatusHandler>();
        services.AddScoped<UserResetPassword.ResetPasswordHandler>();

        services.AddScoped<GetPurchaseOrdersHandler>();
        services.AddScoped<GetPurchaseOrderHandler>();
        services.AddScoped<CreatePurchaseOrderHandler>();
        services.AddScoped<SubmitPurchaseOrderHandler>();
        services.AddScoped<ApprovePurchaseOrderHandler>();
        services.AddScoped<ReceivePurchaseOrderHandler>();

        services.AddScoped<GetInventoryValuationHandler>();

        services.AddScoped<GetProfileHandler>();
        services.AddScoped<UpdateProfileHandler>();
        services.AddScoped<ChangePasswordHandler>();
        services.AddScoped<ForgotPasswordHandler>();
        services.AddScoped<AccountResetPassword.ResetPasswordHandler>();
        services.AddScoped<RequestEmailVerificationHandler>();
        services.AddScoped<ConfirmEmailHandler>();
        services.AddScoped<GetTwoFactorStatusHandler>();
        services.AddScoped<SetupTwoFactorHandler>();
        services.AddScoped<VerifyTwoFactorHandler>();
        services.AddScoped<GenerateTwoFactorRecoveryCodesHandler>();
        services.AddScoped<DisableTwoFactorHandler>();
        services.AddScoped<RegenerateTwoFactorRecoveryCodesHandler>();

        services.AddValidatorsFromAssembly(
            typeof(ServiceCollectionExtensions).Assembly);

        return services;
    }
}