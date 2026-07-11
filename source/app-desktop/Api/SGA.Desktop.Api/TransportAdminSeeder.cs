using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SGA.Identity.Constants;
using SGA.Identity.Entities;

namespace SGA.Desktop.Api;

// Bootstraps the very first TransportAdmin account — there is no public register endpoint on this
// API (accounts are provisioned by an already-authenticated admin), so the first one is a one-time
// startup seed, guarded by "only if zero TransportAdmin users exist yet".
public static class TransportAdminSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        if (!await roleManager.RoleExistsAsync(RoleNames.TransportAdmin))
        {
            await roleManager.CreateAsync(new ApplicationRole(RoleNames.TransportAdmin));
        }

        var existingAdmins = await userManager.GetUsersInRoleAsync(RoleNames.TransportAdmin);
        if (existingAdmins.Count > 0)
        {
            return;
        }

        var email = configuration["Seed:TransportAdminEmail"];
        var password = configuration["Seed:TransportAdminPassword"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "No TransportAdmin account exists yet and Seed:TransportAdminEmail/Seed:TransportAdminPassword " +
                "are not configured. Set them via user-secrets to bootstrap the first administrator.");
            return;
        }

        var admin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = "Transport",
            LastName = "Administrator",
            CreatedAtUtc = DateTime.UtcNow,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, RoleNames.TransportAdmin);
            logger.LogInformation("Seeded the initial TransportAdmin account for {Email}", email);
        }
        else
        {
            logger.LogError(
                "Failed to seed the initial TransportAdmin account: {Errors}",
                string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }
}
