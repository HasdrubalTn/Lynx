// <copyright file="IClientConfigurationService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace IdentityService.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IdentityService.Models.Configuration;

/// <summary>
/// Service for managing OAuth client configurations.
/// Provides CRUD operations for client management with DPoP support.
/// </summary>
public interface IClientConfigurationService
{
    /// <summary>
    /// Retrieves a client configuration by its unique identifier.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The client configuration if found, otherwise null.</returns>
    Task<ClientDto?> GetByIdAsync(string clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all client configurations.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of all client configurations.</returns>
    Task<IEnumerable<ClientDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new client configuration.
    /// </summary>
    /// <param name="client">The client configuration to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created client configuration.</returns>
    Task<ClientDto> CreateAsync(ClientDto client, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing client configuration.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="client">The updated client configuration.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated client configuration if found, otherwise null.</returns>
    Task<ClientDto?> UpdateAsync(string clientId, ClientDto client, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a client configuration.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the client was deleted, false if not found.</returns>
    Task<bool> DeleteAsync(string clientId, CancellationToken cancellationToken = default);
}
