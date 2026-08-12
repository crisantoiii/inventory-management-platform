using InventoryPlatform.Web.Authorization;

namespace InventoryPlatform.Web.Middleware;

public sealed class PasswordChangeMiddleware
{
    private readonly RequestDelegate _next;

    public PasswordChangeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path;

        // Allow static assets to pass through.
        if (Path.HasExtension(path))
        {
            await _next(context);
            return;
        }

        if (!context.User.HasClaim(
                IdentityClaimTypes.MustChangePassword,
                bool.TrueString))
        {
            await _next(context);
            return;
        }

        var isChangePasswordPage =
            path.StartsWithSegments("/Account/ChangePassword");

        var isLogoutPage =
            path.StartsWithSegments("/Identity/Account/Logout");

        if (!isChangePasswordPage && !isLogoutPage)
        {
            context.Response.Redirect("/Account/ChangePassword");
            return;
        }

        await _next(context);
    }
}