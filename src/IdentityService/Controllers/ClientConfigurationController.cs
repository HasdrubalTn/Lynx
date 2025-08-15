// <copyright file="ClientConfigurationController.cs" company="PlaceholderCompany">
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
/// Controller for managing OAuth client configurations.
/// Provides CRUD endpoints for client management with admin authorization.
/// </summary>
[ApiController]
[Route("api/configuration/clients")]
[Authorize(Roles = "admin")]
public class ClientConfigurationController : ControllerBase
{
    private readonly IClientConfigurationService clientService;
    private readonly ILogger<ClientConfigurationController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientConfigurationController"/> class.
    /// </summary>
    /// <param name="clientService">The client configuration service.</param>
    /// <param name="logger">The logger instance.</param>
    public ClientConfigurationController(IClientConfigurationService clientService, ILogger<ClientConfigurationController> logger)
    {
        this.clientService = clientService ?? throw new System.ArgumentNullException(nameof(clientService));
        this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all client configurations.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of client configurations.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        this.logger.LogDebug("Getting all client configurations");
        var clients = await this.clientService.GetAllAsync(cancellationToken);
        return this.Ok(clients);
    }

    /// <summary>
    /// Gets a client configuration by ID.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The client configuration if found.</returns>
    [HttpGet("{clientId}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ClientDto>> GetByIdAsync(string clientId, CancellationToken cancellationToken = default)
    {
        using var scope = this.logger.BeginScope("ClientId:{ClientId}", clientId);
        this.logger.LogDebug("Getting client configuration");

        var client = await this.clientService.GetByIdAsync(clientId, cancellationToken);
        if (client == null)
        {
            this.logger.LogWarning("Client not found");
            return this.NotFound($"Client '{clientId}' not found");
        }

        return this.Ok(client);
    }

    /// <summary>
    /// Creates a new client configuration.
    /// </summary>
    /// <param name="client">The client configuration to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created client configuration.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClientDto>> CreateAsync([FromBody] ClientDto client, CancellationToken cancellationToken = default)
    {
        using var scope = this.logger.BeginScope("ClientId:{ClientId}", client.Id);
        this.logger.LogDebug("Creating client configuration");

        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        try
        {
            var createdClient = await this.clientService.CreateAsync(client, cancellationToken);
            this.logger.LogInformation("Client configuration created successfully");
            return this.CreatedAtAction(nameof(this.GetByIdAsync), new { clientId = createdClient.Id }, createdClient);
        }
        catch (System.InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            this.logger.LogWarning(ex, "Client already exists");
            return this.Conflict($"Client '{client.Id}' already exists");
        }
    }

    /// <summary>
    /// Updates an existing client configuration.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="client">The updated client configuration.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The modified client configuration.</returns>
    [HttpPut("{clientId}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ClientDto>> UpdateAsync(string clientId, [FromBody] ClientDto client, CancellationToken cancellationToken = default)
    {
        using var scope = this.logger.BeginScope("ClientId:{ClientId}", clientId);
        this.logger.LogDebug("Updating client configuration");

        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        if (clientId != client.Id)
        {
            this.logger.LogWarning("Client ID mismatch in URL and body");
            return this.BadRequest("Client ID in URL does not match client ID in request body");
        }

        var updatedClient = await this.clientService.UpdateAsync(clientId, client, cancellationToken);
        if (updatedClient == null)
        {
            this.logger.LogWarning("Client not found for update");
            return this.NotFound($"Client '{clientId}' not found");
        }

        this.logger.LogInformation("Client configuration updated successfully");
        return this.Ok(updatedClient);
    }

    /// <summary>
    /// Deletes a client configuration.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("{clientId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteAsync(string clientId, CancellationToken cancellationToken = default)
    {
        using var scope = this.logger.BeginScope("ClientId:{ClientId}", clientId);
        this.logger.LogDebug("Deleting client configuration");

        var deleted = await this.clientService.DeleteAsync(clientId, cancellationToken);
        if (!deleted)
        {
            this.logger.LogWarning("Client not found for deletion");
            return this.NotFound($"Client '{clientId}' not found");
        }

        this.logger.LogInformation("Client configuration deleted successfully");
        return this.NoContent();
    }
}
