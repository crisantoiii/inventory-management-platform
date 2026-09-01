using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        await SeedUserAsync(userManager, 
            IdentityConstants.DefaultAdmin.UserName, 
            IdentityConstants.DefaultAdmin.Email,
            IdentityConstants.DefaultAdmin.Password,
            IdentityConstants.Roles.Administrator
             );

        await SeedUserAsync(userManager,
            IdentityConstants.DefaultManager.UserName,
            IdentityConstants.DefaultManager.Email,
            IdentityConstants.DefaultManager.Password,
            IdentityConstants.Roles.InventoryManager
             );

        await SeedUserAsync(userManager,
            IdentityConstants.DefaultViewer.UserName,
            IdentityConstants.DefaultViewer.Email,
            IdentityConstants.DefaultViewer.Password,
            IdentityConstants.Roles.Viewer
             );

        var context = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        await AuthorizationSeeder.SeedAsync(context);

        await AssignUsersToGroupsAsync(userManager, context);
    }

    private static async Task AssignUsersToGroupsAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        var userGroupMappings = new (string Email, string GroupName)[]
        {
            (IdentityConstants.DefaultAdmin.Email, IdentityConstants.Roles.Administrator),
            (IdentityConstants.DefaultManager.Email, IdentityConstants.Roles.InventoryManager),
            (IdentityConstants.DefaultViewer.Email, IdentityConstants.Roles.Viewer)
        };

        var hasChanges = false;

        foreach (var mapping in userGroupMappings)
        {
            var user = await userManager.FindByEmailAsync(mapping.Email);
            if (user is null)
            {
                continue;
            }

            var group = await context.AuthorizationGroups
                .Include(g => g.UserGroups)
                .SingleOrDefaultAsync(g => g.Name == mapping.GroupName);

            if (group is null)
            {
                continue;
            }

            var alreadyAssigned = await context.UserAuthorizationGroups
                .AnyAsync(ug =>
                    ug.UserId == user.Id &&
                    ug.AuthorizationGroupId == group.Id);

            if (alreadyAssigned)
            {
                continue;
            }

            group.AssignUser(user.Id);
            hasChanges = true;
        }

        if (hasChanges)
        {
            await context.SaveChangesAsync();
        }
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

    private static async Task SeedUserAsync(
        UserManager<ApplicationUser> userManager, 
        string username, 
        string email, 
        string password,
        string role,
        bool isActive = true)
    {
        var user = await userManager.FindByEmailAsync(
            email);

        if ( user is not null)
        {
            return;
        }

        user = new ApplicationUser
        {
            UserName = username,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(
            user,
            password);

        EnsureSucceeded(result, "Creating administrator");

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    Environment.NewLine,
                    result.Errors.Select(e => e.Description)));
        }


        var roleResult = await userManager.AddToRoleAsync(
            user,
            role);

        EnsureSucceeded(roleResult, "Assigning administrator role");

        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    Environment.NewLine,
                    roleResult.Errors.Select(e => e.Description)));
        }
    }

    private static void EnsureSucceeded(
    IdentityResult result,
    string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        throw new InvalidOperationException(
            $"{operation} failed.{Environment.NewLine}" +
            string.Join(
                Environment.NewLine,
                result.Errors.Select(e => e.Description)));
    }
}