// <copyright file="ClientDto.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace IdentityService.Models.Configuration;

using System;

/// <summary>
/// Data Transfer Object for OAuth client configuration.
/// Used by the Configuration Management API for admin operations.
/// </summary>
public sealed record ClientDto
{
    /// <summary>
    /// Gets the unique client identifier.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the human-readable client name.
    /// </summary>
    public required string ClientName { get; init; }

    /// <summary>
    /// Gets the client description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets a value indicating whether this client requires DPoP (Demonstration of Proof-of-Possession).
    /// When true, the client must provide DPoP proofs with token requests and API calls.
    /// </summary>
    public required bool RequireDPoP { get; init; }

    /// <summary>
    /// Gets the allowed grant types (e.g., "authorization_code", "client_credentials").
    /// </summary>
    public string[] AllowedGrantTypes { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the allowed OAuth scopes for this client.
    /// </summary>
    public required string[] AllowedScopes { get; init; }

    /// <summary>
    /// Gets the valid redirect URIs for authorization code flow.
    /// </summary>
    public required string[] RedirectUris { get; init; }

    /// <summary>
    /// Gets the valid post-logout redirect URIs.
    /// </summary>
    public string[] PostLogoutRedirectUris { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the allowed CORS origins for this client.
    /// </summary>
    public string[] AllowedCorsOrigins { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets a value indicating whether the client is enabled and can request tokens.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether the client requires a client secret.
    /// </summary>
    public bool RequireClientSecret { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether the client requires PKCE (Proof Key for Code Exchange).
    /// </summary>
    public bool RequirePkce { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether plain text PKCE is allowed.
    /// </summary>
    public bool AllowPlainTextPkce { get; init; } = false;

    /// <summary>
    /// Gets a value indicating whether the client requires request objects.
    /// </summary>
    public bool RequireRequestObject { get; init; } = false;

    /// <summary>
    /// Gets a value indicating whether access tokens can be transmitted via browser.
    /// </summary>
    public bool AllowAccessTokensViaBrowser { get; init; } = false;

    /// <summary>
    /// Gets the access token lifetime in seconds.
    /// </summary>
    public int AccessTokenLifetime { get; init; } = 3600;

    /// <summary>
    /// Gets the refresh token usage setting.
    /// </summary>
    public int RefreshTokenUsage { get; init; } = 1;

    /// <summary>
    /// Gets the refresh token expiration setting.
    /// </summary>
    public int RefreshTokenExpiration { get; init; } = 1;

    /// <summary>
    /// Gets the absolute refresh token lifetime in seconds.
    /// </summary>
    public int AbsoluteRefreshTokenLifetime { get; init; } = 2592000;

    /// <summary>
    /// Gets the sliding refresh token lifetime in seconds.
    /// </summary>
    public int SlidingRefreshTokenLifetime { get; init; } = 1296000;
}
