// <copyright file="ProfileService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace IdentityService.Services;

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;

/// <summary>
/// Custom profile service to include role claims in tokens.
/// </summary>
public sealed class ProfileService : IProfileService
{
    private readonly UserManager<ApplicationUser> userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileService"/> class.
    /// </summary>
    /// <param name="userManager">The user manager.</param>
    public ProfileService(UserManager<ApplicationUser> userManager)
    {
        this.userManager = userManager;
    }

    /// <summary>
    /// Gets the profile data for the user.
    /// </summary>
    /// <param name="context">The profile data request context.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var user = await this.userManager.GetUserAsync(context.Subject);
        if (user == null)
        {
            return;
        }

        var roles = await this.userManager.GetRolesAsync(user);
        var claims = new List<Claim>();

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Add user claims
        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
        claims.Add(new Claim(ClaimTypes.Name, user.UserName ?? string.Empty));

        context.IssuedClaims = claims;
    }

    /// <summary>
    /// Indicates if the user is active.
    /// </summary>
    /// <param name="context">The is active context.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task IsActiveAsync(IsActiveContext context)
    {
        var user = await this.userManager.GetUserAsync(context.Subject);
        context.IsActive = user != null;
    }
}
