// <copyright file="ApiScopeDto.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace IdentityService.Models.Configuration;

using System;
using System.Collections.Generic;

/// <summary>
/// Data Transfer Object for API scope configuration.
/// Represents OAuth/OIDC scopes that clients can request.
/// </summary>
public sealed record ApiScopeDto
{
    /// <summary>
    /// Gets the unique scope name (e.g., "api1", "user.read").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the human-readable display name shown in consent screens.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets the detailed description of what this scope allows.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets a value indicating whether this scope is required and cannot be deselected by users.
    /// </summary>
    public required bool Required { get; init; }

    /// <summary>
    /// Gets a value indicating whether this scope is enabled and can be requested.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether this scope should be emphasized in consent screens.
    /// </summary>
    public bool Emphasize { get; init; }

    /// <summary>
    /// Gets a value indicating whether this scope appears in the discovery document.
    /// </summary>
    public bool ShowInDiscoveryDocument { get; init; } = true;

    /// <summary>
    /// Gets the user claims that should be included when this scope is requested.
    /// </summary>
    public string[] UserClaims { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets custom properties for the scope.
    /// </summary>
    public Dictionary<string, string> Properties { get; init; } = new();
}
