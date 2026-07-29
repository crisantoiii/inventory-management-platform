using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryPlatform.Infrastructure.Identity;

public static class IdentitySeeder
{

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        await SeedRolesAsync(roleManager);

        await SeedAdministratorAsync(userManager);
    }

    private static async Task SeedRolesAsync(
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        var roles = new[]
        {
            IdentityConstants.Roles.Administrator,
            IdentityConstants.Roles.InventoryManager,
            IdentityConstants.Roles.Viewer
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>
                {
                    Name = role
                });
            }
        }
    }

    private static async Task SeedAdministratorAsync(
        UserManager<ApplicationUser> userManager)
    {
        var admin = await userManager.FindByEmailAsync(
            IdentityConstants.DefaultAdmin.Email);

        if (admin is not null)
        {
            return;
        }

        admin = new ApplicationUser
        {
            UserName = IdentityConstants.DefaultAdmin.UserName,
            Email = IdentityConstants.DefaultAdmin.Email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(
            admin,
            IdentityConstants.DefaultAdmin.Password);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    Environment.NewLine,
                    result.Errors.Select(e => e.Description)));
        }

        var roleResult = await userManager.AddToRoleAsync(
            admin,
            IdentityConstants.Roles.Administrator);

        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    Environment.NewLine,
                    roleResult.Errors.Select(e => e.Description)));
        }
    }
}