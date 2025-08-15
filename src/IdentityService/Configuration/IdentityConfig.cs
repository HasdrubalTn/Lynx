// <copyright file="IdentityConfig.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace IdentityService.Configuration;

using System.Collections.Generic;
using Duende.IdentityServer.Models;

/// <summary>
/// IdentityServer configuration for clients, resources, and scopes.
/// </summary>
public static class IdentityConfig
{
    /// <summary>
    /// Gets the identity resources.
    /// </summary>
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new("roles", "User Roles", new[] { "role" }),
        };

    /// <summary>
    /// Gets the API scopes.
    /// </summary>
    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new("lynx_api", "Lynx API Access"),
        };

    /// <summary>
    /// Gets the API resources.
    /// </summary>
    public static IEnumerable<ApiResource> ApiResources =>
        new ApiResource[]
        {
            new("lynx_api", "Lynx API")
            {
                Scopes = { "lynx_api" },
                UserClaims = { "role" },
            },
        };

    /// <summary>
    /// Gets the clients configuration.
    /// </summary>
    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // WebApp SPA Client
            new()
            {
                ClientId = "lynx-webapp",
                ClientName = "Lynx Web Application",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = false,
                AllowOfflineAccess = true,

                RedirectUris = { "http://localhost:3000/signin-oidc", "https://app.lynx.com/signin-oidc" },
                PostLogoutRedirectUris = { "http://localhost:3000", "https://app.lynx.com" },
                AllowedCorsOrigins = { "http://localhost:3000", "https://app.lynx.com" },

                AllowedScopes =
                {
                    "openid",
                    "profile",
                    "roles",
                    "lynx_api",
                },
            },

            // AdminApp SPA Client
            new()
            {
                ClientId = "lynx-adminapp",
                ClientName = "Lynx Admin Application",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = false,
                AllowOfflineAccess = true,

                RedirectUris = { "http://localhost:3001/signin-oidc", "https://admin.lynx.com/signin-oidc" },
                PostLogoutRedirectUris = { "http://localhost:3001", "https://admin.lynx.com" },
                AllowedCorsOrigins = { "http://localhost:3001", "https://admin.lynx.com" },

                AllowedScopes =
                {
                    "openid",
                    "profile",
                    "roles",
                    "lynx_api",
                },
            },
        };
}
