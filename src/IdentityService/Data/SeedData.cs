// <copyright file="SeedData.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace IdentityService.Data;

using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

/// <summary>
/// Seeds initial data for development.
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Seeds the admin user for local development.
    /// </summary>
    /// <param name="userManager">The user manager.</param>
    /// <param name="roleManager">The role manager.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task SeedAdminUserAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger logger)
    {
        using var _ = logger.BeginScope("SeedAdminUser");

        // Create admin role
        const string adminRole = "admin";
        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            logger.LogInformation("Creating admin role");
            await roleManager.CreateAsync(new IdentityRole(adminRole));
        }

        // Create admin user
        const string adminEmail = "admin@lynx.local";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        
        if (adminUser == null)
        {
            logger.LogInformation("Creating admin user: {AdminEmail}", adminEmail);
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
            };

            var password = Environment.GetEnvironmentVariable("LYNX_ADMIN_PASSWORD") ?? "Admin123!";
            var result = await userManager.CreateAsync(adminUser, password);
            
            if (result.Succeeded)
            {
                logger.LogInformation("Admin user created successfully");
            }
            else
            {
                logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                return;
            }
        }

        // Add admin role to user
        if (!await userManager.IsInRoleAsync(adminUser, adminRole))
        {
            logger.LogInformation("Adding admin role to user: {AdminEmail}", adminEmail);
            await userManager.AddToRoleAsync(adminUser, adminRole);
        }

        logger.LogInformation("Admin user seeding completed");
    }
}
