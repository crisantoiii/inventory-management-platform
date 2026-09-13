using InventoryPlatform.Infrastructure.Persistence.Context;
using InventoryPlatform.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using LocalIdentity = InventoryPlatform.Infrastructure.Identity;
using InventoryPlatform.Web.Reports.Excel;
using InventoryPlatform.Web.Reports.Pdf;

namespace InventoryPlatform.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWeb(
        this IServiceCollection services)
    {
        services
            .AddIdentity<LocalIdentity.ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;

                options.User.RequireUniqueEmail = true;

                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<
            IUserClaimsPrincipalFactory<LocalIdentity.ApplicationUser>,
            ApplicationUserClaimsPrincipalFactory>();

        services.AddScoped<
            IAuthorizationHandler,
            CapabilityAuthorizationHandler>();

        services.AddScoped<
            IAuthorizationHandler,
            MultiCapabilityAuthorizationHandler>();

        services.AddScoped<ExcelReportWriter>();
        services.AddScoped<PdfReportWriter>();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Identity/Account/Login";
            options.LogoutPath = "/Identity/Account/Logout";
            options.AccessDeniedPath = "/Identity/Account/AccessDenied";

            options.Cookie.Name = "InventoryPlatform.Auth";

            options.SlidingExpiration = true;

            options.ExpireTimeSpan = TimeSpan.FromHours(8);
        });

        services.AddAntiforgery(options =>
        {
            options.Cookie.Name = "InventoryPlatform.AntiForgery";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
        });

        services.AddRazorPages(options =>
        {
            options.Conventions.AuthorizeFolder("/");

            options.Conventions.AllowAnonymousToPage("/Index");

            options.Conventions.AllowAnonymousToPage("/Account/ForgotPassword");
            options.Conventions.AllowAnonymousToPage("/Account/ResetPassword");
            options.Conventions.AllowAnonymousToPage("/Account/TwoFactorLogin");

            options.Conventions.AllowAnonymousToAreaPage(
                "Identity",
                "/Account/Login");
        });

        services.AddRazorPages(options =>
        {
            options.Conventions.AuthorizeFolder("/Products");

            options.Conventions.AuthorizeFolder(
                "/Administration",
                AuthorizationPolicies.Administrator);

            options.Conventions.AuthorizeFolder(
                "/Inventory",
                AuthorizationPolicies.InventoryManagement);
        });

        services.AddAuthorization(options =>
        {
            options.AddCapabilityPolicy(
                AuthorizationPolicies.Administrator,
                AuthorizationPolicies.AdministrationAccess);

            options.AddCapabilityPolicy(
                AuthorizationPolicies.InventoryManagement,
                AuthorizationPolicies.InventoryManagementCapabilities.ProductCreate,
                AuthorizationPolicies.InventoryManagementCapabilities.ProductEdit,
                AuthorizationPolicies.InventoryManagementCapabilities.CategoryCreate,
                AuthorizationPolicies.InventoryManagementCapabilities.SupplierCreate,
                AuthorizationPolicies.InventoryManagementCapabilities.SupplierEdit,
                AuthorizationPolicies.InventoryManagementCapabilities.CustomerCreate,
                AuthorizationPolicies.InventoryManagementCapabilities.CustomerEdit,
                AuthorizationPolicies.InventoryManagementCapabilities.UnitEdit,
                AuthorizationPolicies.InventoryManagementCapabilities.InventoryTransactionCreate);

            options.AddCapabilityPolicy(
                AuthorizationPolicies.ViewInventory,
                AuthorizationPolicies.ViewInventoryCapabilities.DashboardView,
                AuthorizationPolicies.ViewInventoryCapabilities.ProductView,
                AuthorizationPolicies.ViewInventoryCapabilities.CategoryView,
                AuthorizationPolicies.ViewInventoryCapabilities.UnitView,
                AuthorizationPolicies.ViewInventoryCapabilities.CustomerView,
                AuthorizationPolicies.ViewInventoryCapabilities.SupplierView,
                AuthorizationPolicies.ViewInventoryCapabilities.InventoryTransactionView);

            options.AddCapabilityPolicy(
                AuthorizationPolicies.ForCapability(AuthorizationPolicies.PurchaseOrder.View),
                AuthorizationPolicies.PurchaseOrder.View);

            options.AddCapabilityPolicy(
                AuthorizationPolicies.ForCapability(AuthorizationPolicies.PurchaseOrder.Create),
                AuthorizationPolicies.PurchaseOrder.Create);

            options.AddCapabilityPolicy(
                AuthorizationPolicies.ForCapability(AuthorizationPolicies.PurchaseOrder.Edit),
                AuthorizationPolicies.PurchaseOrder.Edit);

            options.AddCapabilityPolicy(
                AuthorizationPolicies.ForCapability(AuthorizationPolicies.PurchaseOrder.Submit),
                AuthorizationPolicies.PurchaseOrder.Submit);

            options.AddCapabilityPolicy(
                AuthorizationPolicies.ForCapability(AuthorizationPolicies.PurchaseOrder.Approve),
                AuthorizationPolicies.PurchaseOrder.Approve);

            options.AddCapabilityPolicy(
                AuthorizationPolicies.ForCapability(AuthorizationPolicies.PurchaseOrder.Receive),
                AuthorizationPolicies.PurchaseOrder.Receive);

            options.AddCapabilityPolicy(
                AuthorizationPolicies.ForCapability(AuthorizationPolicies.PurchaseOrder.Cancel),
                AuthorizationPolicies.PurchaseOrder.Cancel);
        });

        services.AddRazorPages();

        return services;
    }
}
