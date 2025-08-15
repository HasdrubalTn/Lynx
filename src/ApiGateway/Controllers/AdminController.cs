// <copyright file="AdminController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ApiGateway.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

/// <summary>
/// Controller for admin-only endpoints.
/// </summary>
[ApiController]
[Route("admin")]
[Authorize(Roles = "admin")]
public sealed class AdminController : ControllerBase
{
    private readonly ILogger<AdminController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminController"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public AdminController(ILogger<AdminController> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Gets admin data - requires admin role.
    /// </summary>
    /// <returns>Admin data response.</returns>
    [HttpGet("data")]
    public IActionResult GetAdminData()
    {
        using var scope = this.logger.BeginScope("GetAdminData");

        var adminData = new
        {
            Message = "Admin data access successful",
            UserId = this.User.Identity?.Name,
        };

        this.logger.LogInformation("Admin data accessed by {UserId}", this.User.Identity?.Name);
        return this.Ok(adminData);
    }
}
