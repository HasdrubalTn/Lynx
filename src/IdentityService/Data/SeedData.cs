// <copyright file="SeedData.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace IdentityService.Data;

using System;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using IdentityService.Configuration;
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

    /// <summary>
    /// Seeds initial IdentityServer configuration data.
    /// </summary>
    /// <param name="context">The configuration database context.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task SeedIdentityServerDataAsync(
        ConfigurationDbContext context,
        ILogger logger)
    {
        using var scope = logger.BeginScope("SeedIdentityServerData");

        // Seed Clients
        if (!context.Clients.Any())
        {
            logger.LogInformation("Seeding IdentityServer clients");

            foreach (var client in IdentityConfig.Clients)
            {
                context.Clients.Add(client.ToEntity());
            }
        }

        // Seed Identity Resources
        if (!context.IdentityResources.Any())
        {
            logger.LogInformation("Seeding IdentityServer identity resources");

            foreach (var resource in IdentityConfig.IdentityResources)
            {
                context.IdentityResources.Add(resource.ToEntity());
            }
        }

        // Seed API Scopes
        if (!context.ApiScopes.Any())
        {
            logger.LogInformation("Seeding IdentityServer API scopes");

            foreach (var apiScope in IdentityConfig.ApiScopes)
            {
                context.ApiScopes.Add(apiScope.ToEntity());
            }
        }

        // Seed API Resources
        if (!context.ApiResources.Any())
        {
            logger.LogInformation("Seeding IdentityServer API resources");

            foreach (var resource in IdentityConfig.ApiResources)
            {
                context.ApiResources.Add(resource.ToEntity());
            }
        }

        await context.SaveChangesAsync();
        logger.LogInformation("IdentityServer data seeding completed");
    }
}
