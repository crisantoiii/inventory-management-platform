using Microsoft.Extensions.DependencyInjection;
using InventoryPlatform.Infrastructure.Persistence.Context;
using LocalIdentity = InventoryPlatform.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

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

            options.Conventions.AllowAnonymousToAreaPage(
                "Identity",
                "/Account/Login");
        });

        services.AddRazorPages(options =>
        {
            options.Conventions.AuthorizeFolder("/Products");

            options.Conventions.AuthorizeFolder(
                "/Administration",
                LocalIdentity.IdentityConstants.Roles.Administrator);

            options.Conventions.AuthorizeFolder(
                "/Inventory",
                $"{LocalIdentity.IdentityConstants.Roles.Administrator},{LocalIdentity.IdentityConstants.Roles.InventoryManager}");
        });

        services.AddRazorPages();

        return services;
    }
}