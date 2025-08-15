// <copyright file="MeController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ApiGateway.Controllers;

using System.Security.Claims;
using Lynx.Abstractions.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

/// <summary>
/// Controller for user identity endpoints.
/// </summary>
[ApiController]
[Route("api")]
[Authorize]
public sealed class MeController : ControllerBase
{
    private readonly ILogger<MeController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MeController"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public MeController(ILogger<MeController> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Gets the current user information from claims.
    /// </summary>
    /// <returns>User information DTO.</returns>
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        using var scope = this.logger.BeginScope("GetCurrentUser");

        if (!this.User.Identity?.IsAuthenticated ?? true)
        {
            this.logger.LogWarning("Unauthenticated user attempted to access /me endpoint");
            return this.Unauthorized();
        }

        var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var username = this.User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

        // Convert role claims to string array
        var roleClaims = this.User.FindAll(ClaimTypes.Role);
        var tempRoles = new string[10]; // Temporary array, assume max 10 roles
        var roleCount = 0;
        foreach (var roleClaim in roleClaims)
        {
            if (roleCount < tempRoles.Length)
            {
                tempRoles[roleCount++] = roleClaim.Value;
            }
        }

        var roles = new string[roleCount];
        for (int i = 0; i < roleCount; i++)
        {
            roles[i] = tempRoles[i];
        }

        var userInfo = new UserInfoDto
        {
            Id = userId,
            Username = username,
            Roles = roles,
        };

        this.logger.LogInformation("Returning user info for {Username} with {RoleCount} roles", username, roles.Length);
        return this.Ok(userInfo);
    }
}
