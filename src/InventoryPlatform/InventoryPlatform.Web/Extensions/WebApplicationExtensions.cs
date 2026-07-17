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

        app.UseAuthorization();

        app.MapStaticAssets();

        app.MapRazorPages()
            .WithStaticAssets();

        return app;
    }
}