using Microsoft.Extensions.DependencyInjection;
using InventoryPlatform.Infrastructure.Identity;
using InventoryPlatform.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;

namespace InventoryPlatform.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWeb(
        this IServiceCollection services)
    {
        services
            .AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
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

        services.AddRazorPages();

        return services;
    }
}