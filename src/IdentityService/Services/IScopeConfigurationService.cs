// <copyright file="IScopeConfigurationService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace IdentityService.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IdentityService.Models.Configuration;

/// <summary>
/// Service for managing API scope configurations.
/// Provides CRUD operations for API scope management.
/// </summary>
public interface IScopeConfigurationService
{
    /// <summary>
    /// Retrieves an API scope configuration by its name.
    /// </summary>
    /// <param name="scopeName">The scope name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The API scope configuration if found, otherwise null.</returns>
    Task<ApiScopeDto?> GetByNameAsync(string scopeName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all API scope configurations.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of all API scope configurations.</returns>
    Task<IEnumerable<ApiScopeDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new API scope configuration.
    /// </summary>
    /// <param name="scope">The API scope configuration to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created API scope configuration.</returns>
    Task<ApiScopeDto> CreateAsync(ApiScopeDto scope, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing API scope configuration.
    /// </summary>
    /// <param name="scopeName">The scope name.</param>
    /// <param name="scope">The updated API scope configuration.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated API scope configuration if found, otherwise null.</returns>
    Task<ApiScopeDto?> UpdateAsync(string scopeName, ApiScopeDto scope, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an API scope configuration.
    /// </summary>
    /// <param name="scopeName">The scope name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the scope was deleted, false if not found.</returns>
    Task<bool> DeleteAsync(string scopeName, CancellationToken cancellationToken = default);
}
