using InventoryPlatform.Infrastructure.Identity;
using InventoryPlatform.Web.Middleware;
using Microsoft.AspNetCore.Builder;

namespace InventoryPlatform.Web.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseWeb(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();

        app.UseMiddleware<PasswordChangeMiddleware>();

        app.UseAuthorization();

        IdentitySeeder.SeedAsync(app.Services).GetAwaiter().GetResult();

        app.MapStaticAssets();

        app.MapRazorPages()
            .WithStaticAssets();

        return app;
    }
}