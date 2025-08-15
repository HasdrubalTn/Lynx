// <copyright file="ScopeConfigurationController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace IdentityService.Controllers;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IdentityService.Models.Configuration;
using IdentityService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

/// <summary>
/// Controller for managing API scope configurations.
/// Provides CRUD endpoints for scope management with admin authorization.
/// </summary>
[ApiController]
[Route("api/configuration/scopes")]
[Authorize(Roles = "admin")]
public class ScopeConfigurationController : ControllerBase
{
    private readonly IScopeConfigurationService scopeService;
    private readonly ILogger<ScopeConfigurationController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScopeConfigurationController"/> class.
    /// </summary>
    /// <param name="scopeService">The scope configuration service.</param>
    /// <param name="logger">The logger instance.</param>
    public ScopeConfigurationController(IScopeConfigurationService scopeService, ILogger<ScopeConfigurationController> logger)
    {
        this.scopeService = scopeService ?? throw new System.ArgumentNullException(nameof(scopeService));
        this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all API scope configurations.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of API scope configurations.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ApiScopeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<ApiScopeDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        this.logger.LogDebug("Getting all API scope configurations");
        var scopes = await this.scopeService.GetAllAsync(cancellationToken);
        return this.Ok(scopes);
    }

    /// <summary>
    /// Gets an API scope configuration by name.
    /// </summary>
    /// <param name="scopeName">The scope name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The API scope configuration if found.</returns>
    [HttpGet("{scopeName}")]
    [ProducesResponseType(typeof(ApiScopeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiScopeDto>> GetByNameAsync(string scopeName, CancellationToken cancellationToken = default)
    {
        using var scope = this.logger.BeginScope("ScopeName:{ScopeName}", scopeName);
        this.logger.LogDebug("Getting API scope configuration");

        var apiScope = await this.scopeService.GetByNameAsync(scopeName, cancellationToken);
        if (apiScope == null)
        {
            this.logger.LogWarning("API scope not found");
            return this.NotFound($"API scope '{scopeName}' not found");
        }

        return this.Ok(apiScope);
    }

    /// <summary>
    /// Creates a new API scope configuration.
    /// </summary>
    /// <param name="scope">The API scope configuration to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created API scope configuration.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiScopeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiScopeDto>> CreateAsync([FromBody] ApiScopeDto scope, CancellationToken cancellationToken = default)
    {
        using var logScope = this.logger.BeginScope("ScopeName:{ScopeName}", scope.Name);
        this.logger.LogDebug("Creating API scope configuration");

        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        try
        {
            var createdScope = await this.scopeService.CreateAsync(scope, cancellationToken);
            this.logger.LogInformation("API scope configuration created successfully");
            return this.CreatedAtAction(nameof(this.GetByNameAsync), new { scopeName = createdScope.Name }, createdScope);
        }
        catch (System.InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            this.logger.LogWarning(ex, "API scope already exists");
            return this.Conflict($"API scope '{scope.Name}' already exists");
        }
    }

    /// <summary>
    /// Updates an existing API scope configuration.
    /// </summary>
    /// <param name="scopeName">The scope name.</param>
    /// <param name="scope">The updated API scope configuration.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated scope configuration.</returns>
    [HttpPut("{scopeName}")]
    [ProducesResponseType(typeof(ApiScopeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiScopeDto>> UpdateAsync(string scopeName, [FromBody] ApiScopeDto scope, CancellationToken cancellationToken = default)
    {
        using var logScope = this.logger.BeginScope("ScopeName:{ScopeName}", scopeName);
        this.logger.LogDebug("Updating API scope configuration");

        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        if (scopeName != scope.Name)
        {
            this.logger.LogWarning("Scope name mismatch in URL and body");
            return this.BadRequest("Scope name in URL does not match scope name in request body");
        }

        var updatedScope = await this.scopeService.UpdateAsync(scopeName, scope, cancellationToken);
        if (updatedScope == null)
        {
            this.logger.LogWarning("API scope not found for update");
            return this.NotFound($"API scope '{scopeName}' not found");
        }

        this.logger.LogInformation("API scope configuration updated successfully");
        return this.Ok(updatedScope);
    }

    /// <summary>
    /// Deletes an API scope configuration.
    /// </summary>
    /// <param name="scopeName">The scope name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("{scopeName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteAsync(string scopeName, CancellationToken cancellationToken = default)
    {
        using var logScope = this.logger.BeginScope("ScopeName:{ScopeName}", scopeName);
        this.logger.LogDebug("Deleting API scope configuration");

        var deleted = await this.scopeService.DeleteAsync(scopeName, cancellationToken);
        if (!deleted)
        {
            this.logger.LogWarning("API scope not found for deletion");
            return this.NotFound($"API scope '{scopeName}' not found");
        }

        this.logger.LogInformation("API scope configuration deleted successfully");
        return this.NoContent();
    }
}
